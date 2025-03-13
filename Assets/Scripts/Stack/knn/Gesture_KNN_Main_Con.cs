using Leap;
using System.Numerics;
using UnityEngine;

public class Gesture_KNN_Main_Con : MonoBehaviour
{
    private Gesture_KNN_Recognizer knnRecognizer;
    public Gesture_Data_Con dataCollector1;
    public GameObject cube;
    public GameObject cylinder;
    public GameObject sphere;
    public GameObject plane;
    private CarMovement carMovement;
    private string currentGesture = "";  // 当前正在执行的手势
    private bool isGestureRecognized = false; // 当前手势是否被识别

    void Start()
    {

        knnRecognizer = GetComponent<Gesture_KNN_Recognizer>();
        dataCollector1 = GetComponent<Gesture_Data_Con>();
        GameObject car = GameObject.Find("car");
        carMovement = car.GetComponent<CarMovement>();
        // 加载手势数据集（这里传入存储手势模板的文件夹路径）
        knnRecognizer.LoadGestureDataset(Application.dataPath + "/Gesture_Tem");
    }

    void Update()
    {
        // 获取当前帧的手势数据
        Frame frame = new Controller().Frame();

        // 检查是否有手存在
        if (frame.Hands.Count == 0)
        {
            Debug.Log("没有检测到手");
            HandleGestureEnd();  // 手势结束时处理
            return; // 没有手，跳过识别过程
        }

        // 使用 Gesture_Data_Con 收集当前帧的数据
        Hand hand = frame.Hands[0];  // 选择第一个检测到的手
        dataCollector1.On_Collect_Gesture_Data(hand); // 收集手势数据

        // 获取当前手势的特征：手指伸展度、手指角度、手指与掌心的距离、手指尖相互之间的距离
        float fingerExtension = dataCollector1.GetFingerExtension();
        float fingertipsAngle = dataCollector1.GetFingertipsAngle();
        float fingertipsDistance = dataCollector1.GetFingertipsDistance();
        float fingertipsMutualDistance = dataCollector1.GetFingertipsMutualDistance();

        // 识别当前手势
        string recognizedGesture = knnRecognizer.RecognizeGesture(fingerExtension, fingertipsAngle, fingertipsDistance, fingertipsMutualDistance);

        // 如果识别到的手势发生变化
        if (recognizedGesture != currentGesture)
        {
            if (currentGesture != "") // 如果当前手势不为空，意味着有手势正在执行，调用结束手势函数
            {
                HandleGestureEnd();  // 结束当前手势
            }

            HandleGestureStart(recognizedGesture); // 开始新的手势
        }
    }

    // 处理手势开始的逻辑
    private void HandleGestureStart(string recognizedGesture)
    {
        currentGesture = recognizedGesture;
        isGestureRecognized = true;

        // 根据识别的手势执行不同的开始操作
        if (recognizedGesture == "YE")
        {
            plane.GetComponent<Renderer>().material.color = Color.blue;
            Debug.Log("开始执行 YE 手势");
            // 执行 YE 手势开始时的操作j
        }
        else if (recognizedGesture == "fist")
        {
            Debug.Log("开始执行 fist 手势");
            cube.GetComponent<Renderer>().material.color = Color.yellow;
            carMovement.speed = 0;
            // 执行 fist 手势开始时的操作
        }
        else if (recognizedGesture == "zhang")
        {
            Debug.Log("开始执行 Zhang 手势");
            cylinder.GetComponent<Renderer>().material.color = Color.green;
            carMovement.speed = 2;
            // 执行 Zhang 手势开始时的操作
        }
        else if (recognizedGesture == "rock")
        {
            Debug.Log("开始执行 Rock 手势");
            sphere.GetComponent<Renderer>().material.color = Color.red;
            // 执行 Rock 手势开始时的操作
        }
    }

    // 处理手势结束的逻辑
    private void HandleGestureEnd()
    {
        if (isGestureRecognized)
        {
            Debug.Log("结束执行 " + currentGesture + " 手势");
            // 执行手势结束时的操作
            // 例如恢复物体状态或停止某些行为

            // 重置状态
            isGestureRecognized = false;

            // 可以根据当前手势进行相应的结束处理
            if (currentGesture == "YE")
            {
                plane.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("结束 YE 手势");
                // 执行 YE 手势结束时的操作
            }
            else if (currentGesture == "fist")
            {
                cube.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("结束 fist 手势");
                // 执行 fist 手势结束时的操作
            }
            else if (currentGesture == "zhang")
            {
                cylinder.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("结束 Zhang 手势");
                // 执行 Zhang 手势结束时的操作
            }
            else if (currentGesture == "rock")
            {
                sphere.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("结束 Rock 手势");
                // 执行 Rock 手势结束时的操作
            }

            currentGesture = ""; // 清空当前手势
        }
    }
}
