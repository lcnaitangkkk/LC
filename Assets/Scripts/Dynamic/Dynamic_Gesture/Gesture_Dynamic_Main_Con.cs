using UnityEngine;
using Leap;

public class Gesture_Dynamic_Main_Con : MonoBehaviour
{
    private Gesture_Dynamic_Start_End startEndHandler;
    private Gesture_Dynamic_Data_Con dataCollector;
    public Gesture_Dynamic_Template gesture_Template;
    private Controller leapController; // 只创建一个 Leap Controller
    private int frameCounter = 0; // 帧数计数器

    void Start()
    {
        startEndHandler = GetComponent<Gesture_Dynamic_Start_End>();
        dataCollector = GetComponent<Gesture_Dynamic_Data_Con>();
        gesture_Template = GetComponent<Gesture_Dynamic_Template>();

        leapController = new Controller(); // 在 Start 中创建一个 Controller
    }

    void Update()
    {
        Frame frame = leapController.Frame(); // 使用已创建的 Controller 实例获取帧数据

        // 检查手势开始和结束
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // 收集手势数据
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            frameCounter++;
            if (frameCounter % 2 == 0) // 每两帧记录一次数据
            {
                Hand hand = frame.Hands[0]; // 获取第一只手的数据
                dataCollector.On_Collect_Gesture_Data(hand); // 收集手势数据
            }
        }
    }

    // 手势开始
    public void StartGesture()
    {
        dataCollector.On_Initialize_GestureData(); // 初始化手势数据
        frameCounter = 0; // 重置帧数计数器
        Debug.Log("Dynamic Gesture Started");
    }

    // 手势结束
    public void EndGesture()
    {
        Debug.Log("Dynamic Gesture Ended");

        // 保存手势模板
        // 确保数据已经完全收集，在保存之前需要进行必要的验证
        gesture_Template.SaveGestureTemplate(); // 保存手势模板
    }
}