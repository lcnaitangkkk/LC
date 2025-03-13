using UnityEngine;
using System.Collections.Generic;
using Python.Runtime;
using System.IO;
using System.Linq;
using System.Collections;
using System;

// 手势识别类，用于在Unity中实现手势识别功能
public class GestureRecognition_HMM : MonoBehaviour
{
    // 手势数据收集器，用于获取手势相关的数据
    public Gesture_Dynamic_Data_Con gestureDataCollector;
    // 识别间隔时间，用于防止高频调用，单位为秒
    public float recognitionInterval = 0.5f;
    // 数据采集时长，单位为秒
    public float dataCollectionDuration = 2f;

    // 存储手势模型的字典，键为手势标签，值为对应的Python模型对象
    private Dictionary<string, dynamic> gestureModels = new Dictionary<string, dynamic>();
    // 标记Python环境是否已经初始化
    private bool isPythonInitialized = false;

    // 数据缓冲区，存储每个特征的数据和对应的时间戳
    private List<(List<float> data, float timestamp)> dataBuffer = new List<(List<float>, float)>();

    // 启动方法，使用协程异步初始化Python环境，并加载模型
    IEnumerator Start()
    {
        // 异步初始化Python环境
        yield return InitPythonEnvironment();

        // 加载手势模型
        LoadGestureModels();
    }

    // 初始化Python环境的协程方法
    IEnumerator InitPythonEnvironment()
    {
        try
        {
            // 设置 PythonHome 为虚拟环境的根目录
            PythonEngine.PythonHome = @"D:\RuanJian\VsCode\Vscode_code\new_hmm\.venv";

            // 构建 PythonPath，包含虚拟环境的 site-packages 目录
            string venvLibPath = @"D:\RuanJian\VsCode\Vscode_code\new_hmm\.venv\Lib\site-packages";
            string venvDLLsPath = @"D:\RuanJian\VsCode\Vscode_code\new_hmm\.venv\DLLs";
            // Python 安装目录下的 Lib 文件夹
            string systemLibPath = @"D:\RuanJian\Python\py3.7.964\Lib";
            // Python 安装目录下的 DLLs 文件夹
            string systemDLLsPath = @"D:\RuanJian\Python\py3.7.964\DLLs";

            PythonEngine.PythonPath = $"{venvLibPath};{venvDLLsPath};{systemLibPath};{systemDLLsPath}";

            // 初始化 Python 运行时环境
            PythonEngine.Initialize();
            // 标记 Python 环境已初始化
            isPythonInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing Python environment: {e.Message}\n{e.StackTrace}");
        }
        // 暂停协程，下一帧继续执行
        yield return null;
    }

    // 加载手势模型的方法
    void LoadGestureModels()
    {
        // 如果Python环境未初始化，则直接返回
        if (!isPythonInitialized) return;

        string modelsPath = Path.Combine(Application.dataPath, "Models");
        if (!Directory.Exists(modelsPath))
        {
            Debug.LogError($"Models directory not found: {modelsPath}");
            return;
        }

        // 获取Python全局解释器锁（GIL），确保在多线程环境下安全访问Python对象
        using (Py.GIL())
        {
            // 导入Python的joblib模块，用于加载模型
            dynamic joblib = Py.Import("joblib");

            // 遍历模型文件夹下的所有.pkl文件
            foreach (var modelFile in Directory.GetFiles(modelsPath, "*.pkl"))
            {
                if (!File.Exists(modelFile))
                {
                    Debug.LogError($"Model file not found: {modelFile}");
                    continue;
                }

                // 提取模型文件名，去除扩展名和_hmm_model后缀，作为手势标签
                string label = Path.GetFileNameWithoutExtension(modelFile)
                    .Replace("_hmm_model", "");

                try
                {
                    // 使用joblib加载模型
                    dynamic model = joblib.load(modelFile);
                    // 将模型添加到手势模型字典中
                    gestureModels[label] = model;
                    // 打印加载成功的日志
                    Debug.Log($"Loaded model: {label}");
                }
                catch (PythonException e)
                {
                    // 打印加载模型时出现的错误日志
                    Debug.LogError($"Error loading {label}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }

    // 每帧更新方法，进行手势识别
    void Update()
    {
        // 收集数据
        CollectData();

        // 节流控制，根据识别间隔时间判断是否进行识别
        if (Time.time % recognitionInterval > Time.deltaTime) return;

        // 准备输入数据
        float[] inputData = PrepareInputData();
        // 如果输入数据为空，则直接返回
        if (inputData == null) return;

        // 识别手势，返回识别到的手势标签
        string detectedGesture = RecognizeGesture(inputData);
        // 如果识别到有效的手势标签
        if (!string.IsNullOrEmpty(detectedGesture))
        {
            // 执行识别到的手势对应的操作
            ExecuteGestureAction(detectedGesture);
        }
    }

    // 收集数据的方法
    void CollectData()
    {
        List<float> palmMovement = gestureDataCollector.GetPalmMovementList();
        List<float> fingerExtension = gestureDataCollector.GetFingerExtensionList();
        List<float> palmAngle = gestureDataCollector.GetPalmAngleList();
        List<float> movementRate = gestureDataCollector.GetMovementRateList();

        if (palmMovement == null || fingerExtension == null || palmAngle == null || movementRate == null)
        {
            Debug.LogError("One or more data lists are null.");
            return;
        }

        // 组合数据
        List<float> combinedData = new List<float>();
        combinedData.AddRange(palmMovement);
        combinedData.AddRange(fingerExtension);
        combinedData.AddRange(palmAngle);
        combinedData.AddRange(movementRate);

        // 添加到数据缓冲区
        dataBuffer.Add((combinedData, Time.time));

        // 移除超过两秒的数据
        float cutoffTime = Time.time - dataCollectionDuration;
        dataBuffer = dataBuffer.Where(item => item.timestamp >= cutoffTime).ToList();
    }

    // 准备输入数据的方法
    float[] PrepareInputData()
    {
        // 合并缓冲区中的所有数据
        List<float> allData = new List<float>();
        foreach (var item in dataBuffer)
        {
            allData.AddRange(item.data);
        }

        return allData.ToArray();
    }

    // 识别手势的方法，使用维特比算法计算得分
    string RecognizeGesture(float[] inputData)
    {
        // 获取Python全局解释器锁（GIL），确保在多线程环境下安全访问Python对象
        using (Py.GIL())
        {
            // 导入Python的numpy模块
            dynamic np = Py.Import("numpy");
            // 将输入数据转换为numpy数组，并调整形状
            dynamic data = np.array(inputData).reshape(-1, 1);

            // 存储最佳匹配的手势标签
            string bestMatch = "";
            // 存储最大得分
            float maxScore = float.MinValue;

            // 遍历所有手势模型
            foreach (var kvp in gestureModels)
            {
                try
                {
                    // 使用维特比算法计算对数概率
                    dynamic result = kvp.Value.decode(data, algorithm: "viterbi");
                    float score = (float)result[0];

                    // 如果当前得分大于最大得分
                    if (score > maxScore)
                    {
                        // 更新最大得分
                        maxScore = score;
                        // 更新最佳匹配的手势标签
                        bestMatch = kvp.Key;
                    }
                }
                catch (PythonException e)
                {
                    // 打印评估模型时出现的错误日志
                    Debug.LogError($"Error evaluating {kvp.Key}: {e.Message}\n{e.StackTrace}");
                }
            }

            // 根据阈值判断是否返回最佳匹配的手势标签
            return maxScore > -10 ? bestMatch : "";
        }
    }

    // 执行手势动作的方法
    void ExecuteGestureAction(string gesture)
    {
        // 打印识别到的手势标签
        Debug.Log($"Detected gesture: {gesture}");
        switch (gesture)
        {
            case "SwipeRight":
                SwipeRight();
                break;
            case "Fist":
                Fist();
                break;
            case "Wave":
                Wave();
                break;
            default:
                Debug.Log($"No action defined for gesture: {gesture}");
                break;
        }
    }

    // 向右滑动手势对应的函数
    void SwipeRight()
    {
        Debug.Log("Executing SwipeRight action");
        // 这里添加向右滑动手势的具体逻辑
    }

    // 握拳手势对应的函数
    void Fist()
    {
        Debug.Log("Executing Fist action");
        // 这里添加握拳手势的具体逻辑
    }

    // 挥手手势对应的函数
    void Wave()
    {
        Debug.Log("Executing Wave action");
        // 这里添加挥手手势的具体逻辑
    }

    // 销毁方法，在对象销毁时关闭Python环境
    void OnDestroy()
    {
        // 如果Python环境已经初始化
        if (isPythonInitialized)
        {
            // 关闭Python运行时环境
            PythonEngine.Shutdown();
        }
    }
}