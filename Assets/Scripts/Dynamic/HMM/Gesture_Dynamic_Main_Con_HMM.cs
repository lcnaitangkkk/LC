using UnityEngine;
using Leap;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Collections.Generic;

public class Gesture_Dynamic_Main_Con_HMM : MonoBehaviour
{
    // 引用相关组件
    private Gesture_Dynamic_Start_End_HMM startEndHandler; // 用于检测手势的开始和结束
    private Gesture_Dynamic_Data_Con_HMM dataCollector;      // 用于采集手势数据
    public Gesture_Dynamic_Template_HMM gesture_Template;    // 用于保存和加载手势模板
    private Controller leapController;                      // Leap Motion 控制器，用于获取手部数据
    private float timer = 0f;                               // 计时器，用于控制数据采集间隔
    private const float interval = 0.7f;                    // 数据采集时间间隔（0.7秒）
    private bool canSendNextFile = true;                    // 是否可以发送下一个文件，避免重复发送
    private bool isRecording = false;                       // 是否正在录制手势数据
    public float velocityThreshold = 0.5f;                  // 手掌速度阈值，用于判断是否开始录制手势
    public float fingerVelocityThreshold = 0.5f;            // 手指速度阈值，用于判断手指是否移动
    public float transmissionDelay = 2.0f;                  // 数据传输延迟时间（秒）
    public Transform targetObject;                        // 目标对象，用于旋转等操作
    private SynchronizationContext mainThreadContext;     // 主线程上下文，用于更新UI等操作
    private int frameCount = 0;                           // 帧数计数器，用于控制数据采集频率
    public HMM_CAR hMM_CAR;

    // 用于记录每个手指的上一帧位置，key 为手指 ID
    private Dictionary<int, Vector3> previousFingerPositions = new Dictionary<int, Vector3>();

    // 与 Python 服务器通信的管道名称
    private const string PIPE_NAME = "GesturePipe";       // 与 Python 一致

    void Start()
    {
        // 获取组件
        startEndHandler = GetComponent<Gesture_Dynamic_Start_End_HMM>();
        dataCollector = GetComponent<Gesture_Dynamic_Data_Con_HMM>();
        gesture_Template = GetComponent<Gesture_Dynamic_Template_HMM>();

        // 初始化 Leap Motion 控制器
        leapController = new Controller();

        // 获取主线程上下文
        mainThreadContext = SynchronizationContext.Current;
    }

    void Update()
    {
        // 获取当前 Leap Motion 帧
        Frame frame = leapController.Frame();

        // 检查手势的开始和结束
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // 如果手势正在进行且有手部数据
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            Hand hand = frame.Hands[0];
            float palmVelocityMagnitude = hand.PalmVelocity.magnitude; // 获取手掌速度

            // 判断是否有手指移动
            bool fingerMoving = IsAnyFingerMoving(hand);

            // 只有当 canSendNextFile 为 true 时，才开始新的录制
            if (!isRecording && canSendNextFile && (palmVelocityMagnitude > velocityThreshold || fingerMoving))
            {
                isRecording = true;
                timer = 0f;
                frameCount = 0;
                dataCollector.On_Initialize_GestureData();
            }

            if (isRecording)
            {
                frameCount++; // 帧数加1
                // 每两帧采集一次数据
                if (frameCount % 2 == 0)
                {
                    dataCollector.On_Collect_Gesture_Data(hand); // 采集手势数据
                }

                timer += Time.deltaTime;
                // 达到时间间隔后，保存数据并发送到 Python 服务器
                if (timer >= interval && canSendNextFile)
                {
                    timer = 0f;
                    isRecording = false;
                    frameCount = 0;
                    canSendNextFile = false;  // 发送后暂停录制，等待响应
                    _ = SaveAndTestTemplate();
                }
            }
        }
        else
        {
            // 手势结束，停止录制
            isRecording = false;
        }
    }

    // 判断是否有手指移动
    private bool IsAnyFingerMoving(Hand hand)
    {
        bool isMoving = false;
        foreach (Finger finger in hand.fingers)
        {
            Vector3 currentPos = new Vector3(finger.TipPosition.x, finger.TipPosition.y, finger.TipPosition.z);
            if (previousFingerPositions.TryGetValue(finger.Id, out Vector3 prevPos))
            {
                float fingerSpeed = (currentPos - prevPos).magnitude / Time.deltaTime;
                if (fingerSpeed > fingerVelocityThreshold)
                {
                    isMoving = true;
                }
                previousFingerPositions[finger.Id] = currentPos;
            }
            else
            {
                previousFingerPositions[finger.Id] = currentPos;
            }
        }
        return isMoving;
    }

    public void StartGesture()
    {
        Debug.Log("Dynamic Gesture Started");
    }

    public void EndGesture()
    {
        Debug.Log("Dynamic Gesture Ended");
        if (isRecording && canSendNextFile)
        {
            isRecording = false;
            frameCount = 0;
            canSendNextFile = false;
            _ = SaveAndTestTemplate();
        }
    }

    // 保存手势模板并测试的异步方法
    private async Task SaveAndTestTemplate()
    {
        gesture_Template.SaveGestureTemplate();
        await CallPythonServerAsync();
    }

    // 与 Python 服务器通信的异步方法
    private async Task CallPythonServerAsync()
    {
        string filePath = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem_Test/MyDynamicGesture.json";

        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut))
        {
            try
            {
                await pipeClient.ConnectAsync(3000);
                byte[] requestBytes = Encoding.UTF8.GetBytes(filePath);
                await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);
                await pipeClient.FlushAsync();

                byte[] responseBytes = new byte[256];
                int bytesRead = await pipeClient.ReadAsync(responseBytes, 0, responseBytes.Length);
                string result = Encoding.UTF8.GetString(responseBytes, 0, bytesRead).Trim();

                Debug.Log($"Python Response: {result}");
                if (result == "ZuoToYou")
                {
                    mainThreadContext.Post(_ => hMM_CAR.Car_Rotate(), null);
                }
                else if (result == "FangDa")
                {
                    mainThreadContext.Post(_ => hMM_CAR.Camera_Near(), null);
                }
                else if (result == "XiaToShang")
                {
                    mainThreadContext.Post(_ => hMM_CAR.Car_Jump(), null);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Pipe communication error: {ex.Message}");
            }
        }

        // 等待传输延迟时间，准备下一次录制
        await Task.Delay((int)(transmissionDelay * 1000));
        canSendNextFile = true;
    }
}