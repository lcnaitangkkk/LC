using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture_Main_Con : MonoBehaviour
{
    private Gesture_Start_End startEndHandler;
    private Gesture_Data_Con dataCollector;
    public Gesture_Template gesture_Template;

    void Start()
    {
        startEndHandler = GetComponent<Gesture_Start_End>();
        dataCollector = GetComponent<Gesture_Data_Con>();
        gesture_Template = GetComponent<Gesture_Template>();

    }

    void Update()
    {
        Frame frame = new Controller().Frame();

        // 检查手势开始与结束
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // 采集手势数据
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            Hand hand = frame.Hands[0];
            dataCollector.On_Collect_Gesture_Data(hand);
        }
    }

    // 手势开始
    public void StartGesture()
    {
        dataCollector.On_Initialize_GestureData(); // 初始化数据
        Debug.Log("Gesture Started");
    }

    // 手势结束
    public void EndGesture()
    {
        Debug.Log("Gesture Ended");
        gesture_Template.SaveGestureTemplate();
        // 处理手势数据匹配

        //bool matchFound = gestureMatcher.CompareWithPredefinedTemplate(dataCollector.GetFingerExtension(), dataCollector.GetPalmDirection());
        //Debug.Log("matchFound,执行了");
        //if (matchFound)
        //{
        //    Debug.Log("Gesture matched with template.");
        //    Debug.Log("我艹，匹配成功了！！！");
        //}
        //else
        //{
        //    Debug.Log("No match found.");
        //}
    }
}
