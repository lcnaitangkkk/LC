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

        // ������ƿ�ʼ�����
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // �ɼ���������
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            Hand hand = frame.Hands[0];
            dataCollector.On_Collect_Gesture_Data(hand);
        }
    }

    // ���ƿ�ʼ
    public void StartGesture()
    {
        dataCollector.On_Initialize_GestureData(); // ��ʼ������
        Debug.Log("Gesture Started");
    }

    // ���ƽ���
    public void EndGesture()
    {
        Debug.Log("Gesture Ended");
        gesture_Template.SaveGestureTemplate();
        // ������������ƥ��

        //bool matchFound = gestureMatcher.CompareWithPredefinedTemplate(dataCollector.GetFingerExtension(), dataCollector.GetPalmDirection());
        //Debug.Log("matchFound,ִ����");
        //if (matchFound)
        //{
        //    Debug.Log("Gesture matched with template.");
        //    Debug.Log("��ܳ��ƥ��ɹ��ˣ�����");
        //}
        //else
        //{
        //    Debug.Log("No match found.");
        //}
    }
}
