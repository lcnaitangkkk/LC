using UnityEngine;
using System.Collections.Generic;
using Python.Runtime;
using System.IO;
using System.Linq;
using System.Collections;
using System;

// ����ʶ���࣬������Unity��ʵ������ʶ����
public class GestureRecognition_HMM : MonoBehaviour
{
    // ���������ռ��������ڻ�ȡ������ص�����
    public Gesture_Dynamic_Data_Con gestureDataCollector;
    // ʶ����ʱ�䣬���ڷ�ֹ��Ƶ���ã���λΪ��
    public float recognitionInterval = 0.5f;
    // ���ݲɼ�ʱ������λΪ��
    public float dataCollectionDuration = 2f;

    // �洢����ģ�͵��ֵ䣬��Ϊ���Ʊ�ǩ��ֵΪ��Ӧ��Pythonģ�Ͷ���
    private Dictionary<string, dynamic> gestureModels = new Dictionary<string, dynamic>();
    // ���Python�����Ƿ��Ѿ���ʼ��
    private bool isPythonInitialized = false;

    // ���ݻ��������洢ÿ�����������ݺͶ�Ӧ��ʱ���
    private List<(List<float> data, float timestamp)> dataBuffer = new List<(List<float>, float)>();

    // ����������ʹ��Э���첽��ʼ��Python������������ģ��
    IEnumerator Start()
    {
        // �첽��ʼ��Python����
        yield return InitPythonEnvironment();

        // ��������ģ��
        LoadGestureModels();
    }

    // ��ʼ��Python������Э�̷���
    IEnumerator InitPythonEnvironment()
    {
        try
        {
            // ���� PythonHome Ϊ���⻷���ĸ�Ŀ¼
            PythonEngine.PythonHome = @"D:\RuanJian\VsCode\Vscode_code\new_hmm\.venv";

            // ���� PythonPath���������⻷���� site-packages Ŀ¼
            string venvLibPath = @"D:\RuanJian\VsCode\Vscode_code\new_hmm\.venv\Lib\site-packages";
            string venvDLLsPath = @"D:\RuanJian\VsCode\Vscode_code\new_hmm\.venv\DLLs";
            // Python ��װĿ¼�µ� Lib �ļ���
            string systemLibPath = @"D:\RuanJian\Python\py3.7.964\Lib";
            // Python ��װĿ¼�µ� DLLs �ļ���
            string systemDLLsPath = @"D:\RuanJian\Python\py3.7.964\DLLs";

            PythonEngine.PythonPath = $"{venvLibPath};{venvDLLsPath};{systemLibPath};{systemDLLsPath}";

            // ��ʼ�� Python ����ʱ����
            PythonEngine.Initialize();
            // ��� Python �����ѳ�ʼ��
            isPythonInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing Python environment: {e.Message}\n{e.StackTrace}");
        }
        // ��ͣЭ�̣���һ֡����ִ��
        yield return null;
    }

    // ��������ģ�͵ķ���
    void LoadGestureModels()
    {
        // ���Python����δ��ʼ������ֱ�ӷ���
        if (!isPythonInitialized) return;

        string modelsPath = Path.Combine(Application.dataPath, "Models");
        if (!Directory.Exists(modelsPath))
        {
            Debug.LogError($"Models directory not found: {modelsPath}");
            return;
        }

        // ��ȡPythonȫ�ֽ���������GIL����ȷ���ڶ��̻߳����°�ȫ����Python����
        using (Py.GIL())
        {
            // ����Python��joblibģ�飬���ڼ���ģ��
            dynamic joblib = Py.Import("joblib");

            // ����ģ���ļ����µ�����.pkl�ļ�
            foreach (var modelFile in Directory.GetFiles(modelsPath, "*.pkl"))
            {
                if (!File.Exists(modelFile))
                {
                    Debug.LogError($"Model file not found: {modelFile}");
                    continue;
                }

                // ��ȡģ���ļ�����ȥ����չ����_hmm_model��׺����Ϊ���Ʊ�ǩ
                string label = Path.GetFileNameWithoutExtension(modelFile)
                    .Replace("_hmm_model", "");

                try
                {
                    // ʹ��joblib����ģ��
                    dynamic model = joblib.load(modelFile);
                    // ��ģ����ӵ�����ģ���ֵ���
                    gestureModels[label] = model;
                    // ��ӡ���سɹ�����־
                    Debug.Log($"Loaded model: {label}");
                }
                catch (PythonException e)
                {
                    // ��ӡ����ģ��ʱ���ֵĴ�����־
                    Debug.LogError($"Error loading {label}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }

    // ÿ֡���·�������������ʶ��
    void Update()
    {
        // �ռ�����
        CollectData();

        // �������ƣ�����ʶ����ʱ���ж��Ƿ����ʶ��
        if (Time.time % recognitionInterval > Time.deltaTime) return;

        // ׼����������
        float[] inputData = PrepareInputData();
        // �����������Ϊ�գ���ֱ�ӷ���
        if (inputData == null) return;

        // ʶ�����ƣ�����ʶ�𵽵����Ʊ�ǩ
        string detectedGesture = RecognizeGesture(inputData);
        // ���ʶ����Ч�����Ʊ�ǩ
        if (!string.IsNullOrEmpty(detectedGesture))
        {
            // ִ��ʶ�𵽵����ƶ�Ӧ�Ĳ���
            ExecuteGestureAction(detectedGesture);
        }
    }

    // �ռ����ݵķ���
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

        // �������
        List<float> combinedData = new List<float>();
        combinedData.AddRange(palmMovement);
        combinedData.AddRange(fingerExtension);
        combinedData.AddRange(palmAngle);
        combinedData.AddRange(movementRate);

        // ��ӵ����ݻ�����
        dataBuffer.Add((combinedData, Time.time));

        // �Ƴ��������������
        float cutoffTime = Time.time - dataCollectionDuration;
        dataBuffer = dataBuffer.Where(item => item.timestamp >= cutoffTime).ToList();
    }

    // ׼���������ݵķ���
    float[] PrepareInputData()
    {
        // �ϲ��������е���������
        List<float> allData = new List<float>();
        foreach (var item in dataBuffer)
        {
            allData.AddRange(item.data);
        }

        return allData.ToArray();
    }

    // ʶ�����Ƶķ�����ʹ��ά�ر��㷨����÷�
    string RecognizeGesture(float[] inputData)
    {
        // ��ȡPythonȫ�ֽ���������GIL����ȷ���ڶ��̻߳����°�ȫ����Python����
        using (Py.GIL())
        {
            // ����Python��numpyģ��
            dynamic np = Py.Import("numpy");
            // ����������ת��Ϊnumpy���飬��������״
            dynamic data = np.array(inputData).reshape(-1, 1);

            // �洢���ƥ������Ʊ�ǩ
            string bestMatch = "";
            // �洢���÷�
            float maxScore = float.MinValue;

            // ������������ģ��
            foreach (var kvp in gestureModels)
            {
                try
                {
                    // ʹ��ά�ر��㷨�����������
                    dynamic result = kvp.Value.decode(data, algorithm: "viterbi");
                    float score = (float)result[0];

                    // �����ǰ�÷ִ������÷�
                    if (score > maxScore)
                    {
                        // �������÷�
                        maxScore = score;
                        // �������ƥ������Ʊ�ǩ
                        bestMatch = kvp.Key;
                    }
                }
                catch (PythonException e)
                {
                    // ��ӡ����ģ��ʱ���ֵĴ�����־
                    Debug.LogError($"Error evaluating {kvp.Key}: {e.Message}\n{e.StackTrace}");
                }
            }

            // ������ֵ�ж��Ƿ񷵻����ƥ������Ʊ�ǩ
            return maxScore > -10 ? bestMatch : "";
        }
    }

    // ִ�����ƶ����ķ���
    void ExecuteGestureAction(string gesture)
    {
        // ��ӡʶ�𵽵����Ʊ�ǩ
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

    // ���һ������ƶ�Ӧ�ĺ���
    void SwipeRight()
    {
        Debug.Log("Executing SwipeRight action");
        // ����������һ������Ƶľ����߼�
    }

    // ��ȭ���ƶ�Ӧ�ĺ���
    void Fist()
    {
        Debug.Log("Executing Fist action");
        // ���������ȭ���Ƶľ����߼�
    }

    // �������ƶ�Ӧ�ĺ���
    void Wave()
    {
        Debug.Log("Executing Wave action");
        // ������ӻ������Ƶľ����߼�
    }

    // ���ٷ������ڶ�������ʱ�ر�Python����
    void OnDestroy()
    {
        // ���Python�����Ѿ���ʼ��
        if (isPythonInitialized)
        {
            // �ر�Python����ʱ����
            PythonEngine.Shutdown();
        }
    }
}