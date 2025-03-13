using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using UnityEngine.UI;

public class Gesture_Start_End : MonoBehaviour
{
    public bool Is_Gesture_InProgress = false; // 判断手势是否正在进行
    public float Gesture_End_Time;
    public float Gesture_Hold_Time = 1f; // 手势结束的时间阈值
    public Gesture_Main_Con gesture_main_con;
    public Button Gesture_Start;
    public Button Gesture_End;
    public bool Bool_Gesture_Start = false;
    public bool Bool_Gesture_End = false;
    private Vector3 lastPalmPosition; // 存储上一帧的掌心位置
    private float lastTimeStamp; // 存储上一帧的时间戳

    void Start()
    {
        gesture_main_con = GetComponent<Gesture_Main_Con>();
        lastPalmPosition = Vector3.zero;
        lastTimeStamp = Time.time;

        //监听UI
        Gesture_Start.onClick.AddListener(On_Gesture_Start);
        Gesture_End.onClick.AddListener(On_Gesture_End);
    }

    // 检查手势是否开始
    public void On_Check_Gesture_Start(Frame frame)
    {
        if (!Is_Gesture_InProgress && frame.Hands.Count > 0 && Bool_Gesture_Start == true)
        {
            Hand hand = frame.Hands[0]; // 选择第一个手

            // 判断手部是否在视野内并且是否处于静止状态
            if (hand.PalmPosition.magnitude > 0f)
            {
                StartGesture();
            }

            // 更新上一帧数据
            lastPalmPosition = new Vector3(hand.PalmPosition.x, hand.PalmPosition.y, hand.PalmPosition.z);
            lastTimeStamp = Time.time;
        }
    }

    // 检查手势是否结束
    public void On_Check_Gesture_End(Frame frame)
    {
        if (Is_Gesture_InProgress && Bool_Gesture_End == true)
        {
            // 手部离开视野，认为手势结束
            EndGesture();
        }
        else if (Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            Hand hand = frame.Hands[0];

            // 判断手势结束的条件：手部离开视野或手势保持静止一定时间
            if (hand.PalmPosition.magnitude > 0f)
            {
                Gesture_End_Time += Time.deltaTime;
                if (Gesture_End_Time > Gesture_Hold_Time)
                {
                    EndGesture();
                }
            }
            else
            {
                Gesture_End_Time = 0f; // 如果手部离开视野，重置结束计时
            }

            // 更新上一帧数据
            lastPalmPosition = new Vector3(hand.PalmPosition.x, hand.PalmPosition.y, hand.PalmPosition.z);
            lastTimeStamp = Time.time;
        }
    }

    // 手势开始
    private void StartGesture()
    {
        Is_Gesture_InProgress = true;
        gesture_main_con.StartGesture(); // 通知主控制脚本开始采集数据
        Bool_Gesture_Start = false; // 禁止手势重新开始
    }

    // 手势结束
    private void EndGesture()
    {
        Is_Gesture_InProgress = false;
        gesture_main_con.EndGesture(); // 通知主控制脚本结束采集
        Bool_Gesture_End = false; // 禁止手势重新结束
    }

    // UI按钮触发手势开始
    public void On_Gesture_Start()
    {
        if (!Is_Gesture_InProgress) // 防止重复开始
        {
            Bool_Gesture_Start = true;
            Bool_Gesture_End = false;
        }
    }

    // UI按钮触发手势结束
    public void On_Gesture_End()
    {
        if (Is_Gesture_InProgress) // 防止没有开始就结束
        {
            Bool_Gesture_End = true;
            Bool_Gesture_Start = false;
        }
    }
}
