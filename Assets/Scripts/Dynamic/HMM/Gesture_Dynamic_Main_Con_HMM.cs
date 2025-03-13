using UnityEngine;
using Leap;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

public class Gesture_Dynamic_Main_Con_HMM : MonoBehaviour
{
    // 各种组件和变量声明
    private Gesture_Dynamic_Start_End_HMM startEndHandler; // 用于检测手势的开始和结束
    private Gesture_Dynamic_Data_Con_HMM dataCollector; // 用于收集手势数据
    public Gesture_Dynamic_Template_HMM gesture_Template; // 用于保存和测试手势模板
    private Controller leapController; // Leap Motion 控制器，用于获取手势数据
    private float timer = 0f; // 定时器，用于控制每隔一段时间采集数据
    private const float interval = 0.7f; // 设置采样间隔为0.7秒
    private bool canSendNextFile = true; // 是否可以发送下一个文件
    private bool isRecording = false; // 是否正在录制手势数据
    public float velocityThreshold = 0.5f; // 速度阈值，用于判断是否开始录制手势
    public Transform targetObject; // 目标物体，用于旋转
    private SynchronizationContext mainThreadContext; // 主线程的上下文，用于更新UI等操作
    private int frameCount = 0; // 帧计数器，用于控制数据的采样频率

    // 定义命名管道名称，Python 端与此保持一致
    private const string PIPE_NAME = "GesturePipe"; // 与 Python 一致

    // Start 是 Unity 的生命周期方法，在脚本初始化时调用
    void Start()
    {
        // 获取组件
        startEndHandler = GetComponent<Gesture_Dynamic_Start_End_HMM>();
        dataCollector = GetComponent<Gesture_Dynamic_Data_Con_HMM>();
        gesture_Template = GetComponent<Gesture_Dynamic_Template_HMM>();

        // 初始化 Leap Motion 控制器
        leapController = new Controller();

        // 获取主线程上下文，方便后面更新物体旋转等操作
        mainThreadContext = SynchronizationContext.Current;
    }

    // Update 是 Unity 的生命周期方法，在每一帧更新时调用
    void Update()
    {
        // 获取 Leap Motion 当前帧
        Frame frame = leapController.Frame();

        // 检查手势的开始和结束
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // 如果手势正在进行中，并且手部存在
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            // 获取第一个手部数据
            Hand hand = frame.Hands[0];
            float palmVelocityMagnitude = hand.PalmVelocity.magnitude; // 获取手掌速度的大小

            // 如果当前没有正在录制手势，并且手掌速度大于阈值，则开始录制
            if (!isRecording && palmVelocityMagnitude > velocityThreshold)
            {
                isRecording = true;
                timer = 0f;
                frameCount = 0;
                dataCollector.On_Initialize_GestureData(); // 初始化手势数据收集
            }

            // 如果正在录制手势数据
            if (isRecording)
            {
                frameCount++; // 增加帧计数器
                // 每隔两帧采集一次数据
                if (frameCount % 2 == 0)
                {
                    dataCollector.On_Collect_Gesture_Data(hand); // 收集手势数据
                }

                timer += Time.deltaTime; // 增加计时器
                // 如果计时器到达设定的间隔，并且可以发送下一个文件
                if (timer >= interval && canSendNextFile)
                {
                    timer = 0f;
                    isRecording = false; // 结束录制
                    frameCount = 0;
                    canSendNextFile = false; // 禁止立即发送下一个文件
                    _ = SaveAndTestTemplate(); // 保存并测试手势模板
                }
            }
        }
        else
        {
            // 如果手势结束，停止录制
            isRecording = false;
        }
    }

    // 手动触发手势开始
    public void StartGesture()
    {
        Debug.Log("Dynamic Gesture Started");
    }

    // 手动触发手势结束
    public void EndGesture()
    {
        Debug.Log("Dynamic Gesture Ended");
        // 如果正在录制并且可以发送下一个文件
        if (isRecording && canSendNextFile)
        {
            isRecording = false;
            frameCount = 0;
            canSendNextFile = false;
            _ = SaveAndTestTemplate(); // 保存并测试手势模板
        }
    }

    // 异步方法保存并测试手势模板
    private async Task SaveAndTestTemplate()
    {
        gesture_Template.SaveGestureTemplate(); // 保存手势模板
        await CallPythonServerAsync(); // 调用 Python 服务进行测试
    }

    // 异步方法与 Python 服务进行通信
    private async Task CallPythonServerAsync()
    {
        // 手势模板文件路径
        string filePath = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem_Test/MyDynamicGesture.json";

        // 创建命名管道客户端，连接到 Python 端的管道
        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut))
        {
            try
            {
                // 异步连接到管道
                await pipeClient.ConnectAsync(3000);
                byte[] requestBytes = Encoding.UTF8.GetBytes(filePath); // 将文件路径转为字节数组
                await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length); // 写入请求数据
                await pipeClient.FlushAsync(); // 刷新管道数据

                byte[] responseBytes = new byte[256]; // 创建字节数组用于接收响应
                int bytesRead = await pipeClient.ReadAsync(responseBytes, 0, responseBytes.Length); // 读取响应数据
                string result = Encoding.UTF8.GetString(responseBytes, 0, bytesRead).Trim(); // 将字节数组转换为字符串

                Debug.Log($"Python Response: {result}"); // 输出 Python 的响应

                // 如果 Python 返回 "ZuoToYou"，则旋转目标物体
                if (result == "ZuoToYou")
                {
                    mainThreadContext.Post(_ => RotateObject(), null); // 在主线程中调用旋转物体的方法
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Pipe communication error: {ex.Message}"); // 处理管道通信异常
            }
        }

        // 允许发送下一个文件
        canSendNextFile = true;
    }

    // 旋转目标物体
    private void RotateObject()
    {
        // 如果目标物体不为空，则进行旋转
        if (targetObject != null)
        {
            targetObject.Rotate(0f, 30f, 0f, Space.Self); // 以目标物体的局部坐标系旋转
        }
    }
}
