using UnityEngine;
using Leap;
using UnityEngine.UI;

public class Gesture_Dynamic_Start_End : MonoBehaviour
{
    public bool Is_Gesture_InProgress = false; // 判断手势是否正在进行
    public float Gesture_End_Time;
    public float Gesture_Hold_Time = 1f; // 手势保持时间阈值
    public float Gesture_Max_Duration = 0.7f; // 手势最大持续时间
    private float Gesture_Start_Time; // 手势开始的时间
    public Gesture_Dynamic_Main_Con gesture_main_con;
    public Button Gesture_Start;
    public Button Gesture_End;
    public bool Bool_Gesture_Start = false;
    public bool Bool_Gesture_End = false;
    private Vector3 lastPalmPosition; // 存储上一帧掌心位置
    private float lastPalmVelocityMagnitude = 0f; // 掌心速度的大小

    void Start()
    {
        gesture_main_con = GetComponent<Gesture_Dynamic_Main_Con>();
        lastPalmPosition = Vector3.zero;

        // 绑定 UI 按钮事件
        Gesture_Start.onClick.AddListener(On_Gesture_Start);
        Gesture_End.onClick.AddListener(On_Gesture_End);
    }

    // 检查手势是否开始
    public void On_Check_Gesture_Start(Frame frame)
    {
        if (!Is_Gesture_InProgress && frame.Hands.Count > 0 && Bool_Gesture_Start == true)
        {
            Hand hand = frame.Hands[0]; // 选择第一只手

            // 判断手掌是否移动以及是否处于合适状态
            if (hand.PalmPosition.magnitude > 0f)
            {
                StartGesture();
            }

            lastPalmPosition = hand.PalmPosition;
        }
    }

    // 检查手势是否结束
    public void On_Check_Gesture_End(Frame frame)
    {
        if (Is_Gesture_InProgress)
        {
            // 判断是否超过最大记录时间
            if (Time.time - Gesture_Start_Time >= Gesture_Max_Duration)
            {
                EndGesture();
                return;
            }

            if (Bool_Gesture_End == true)
            {
                EndGesture();
            }
            else if (frame.Hands.Count > 0)
            {
                Hand hand = frame.Hands[0];

                // 获取掌心速度的大小
                float palmVelocityMagnitude = hand.PalmVelocity.magnitude;

                // 如果手势开始移动，重置结束时间
                if (palmVelocityMagnitude > 0.1f) // 设置一个最小速度阈值，避免误判
                {
                    Gesture_End_Time = 0f;
                }
                else
                {
                    // 速度小于阈值，开始计时
                    Gesture_End_Time += Time.deltaTime;
                }

                // 如果计时超过设定的阈值，结束手势
                if (Gesture_End_Time >= Gesture_Hold_Time)
                {
                    EndGesture();
                }

                lastPalmPosition = hand.PalmPosition;
            }
        }
    }

    // 手势开始
    private void StartGesture()
    {
        Is_Gesture_InProgress = true;
        Gesture_Start_Time = Time.time; // 记录手势开始时间
        gesture_main_con.StartGesture(); // 通知主控制器手势开始收集数据
        Bool_Gesture_Start = false; // 防止重复开始
    }

    // 手势结束
    private void EndGesture()
    {
        Is_Gesture_InProgress = false;
        gesture_main_con.EndGesture(); // 通知主控制器手势结束收集数据
        Bool_Gesture_End = false; // 防止重复结束
    }

    // UI 按钮事件：开始手势
    public void On_Gesture_Start()
    {
        if (!Is_Gesture_InProgress) // 防止重复开始
        {
            Bool_Gesture_Start = true;
            Bool_Gesture_End = false;
        }
    }

    // UI 按钮事件：结束手势
    public void On_Gesture_End()
    {
        if (Is_Gesture_InProgress) // 防止未开始就结束
        {
            Bool_Gesture_End = true;
            Bool_Gesture_Start = false;
        }
    }
}