using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using UnityEngine.UI;

public class Gesture_Start_End : MonoBehaviour
{
    public bool Is_Gesture_InProgress = false; // �ж������Ƿ����ڽ���
    public float Gesture_End_Time;
    public float Gesture_Hold_Time = 1f; // ���ƽ�����ʱ����ֵ
    public Gesture_Main_Con gesture_main_con;
    public Button Gesture_Start;
    public Button Gesture_End;
    public bool Bool_Gesture_Start = false;
    public bool Bool_Gesture_End = false;
    private Vector3 lastPalmPosition; // �洢��һ֡������λ��
    private float lastTimeStamp; // �洢��һ֡��ʱ���

    void Start()
    {
        gesture_main_con = GetComponent<Gesture_Main_Con>();
        lastPalmPosition = Vector3.zero;
        lastTimeStamp = Time.time;

        //����UI
        Gesture_Start.onClick.AddListener(On_Gesture_Start);
        Gesture_End.onClick.AddListener(On_Gesture_End);
    }

    // ��������Ƿ�ʼ
    public void On_Check_Gesture_Start(Frame frame)
    {
        if (!Is_Gesture_InProgress && frame.Hands.Count > 0 && Bool_Gesture_Start == true)
        {
            Hand hand = frame.Hands[0]; // ѡ���һ����

            // �ж��ֲ��Ƿ�����Ұ�ڲ����Ƿ��ھ�ֹ״̬
            if (hand.PalmPosition.magnitude > 0f)
            {
                StartGesture();
            }

            // ������һ֡����
            lastPalmPosition = new Vector3(hand.PalmPosition.x, hand.PalmPosition.y, hand.PalmPosition.z);
            lastTimeStamp = Time.time;
        }
    }

    // ��������Ƿ����
    public void On_Check_Gesture_End(Frame frame)
    {
        if (Is_Gesture_InProgress && Bool_Gesture_End == true)
        {
            // �ֲ��뿪��Ұ����Ϊ���ƽ���
            EndGesture();
        }
        else if (Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            Hand hand = frame.Hands[0];

            // �ж����ƽ������������ֲ��뿪��Ұ�����Ʊ��־�ֹһ��ʱ��
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
                Gesture_End_Time = 0f; // ����ֲ��뿪��Ұ�����ý�����ʱ
            }

            // ������һ֡����
            lastPalmPosition = new Vector3(hand.PalmPosition.x, hand.PalmPosition.y, hand.PalmPosition.z);
            lastTimeStamp = Time.time;
        }
    }

    // ���ƿ�ʼ
    private void StartGesture()
    {
        Is_Gesture_InProgress = true;
        gesture_main_con.StartGesture(); // ֪ͨ�����ƽű���ʼ�ɼ�����
        Bool_Gesture_Start = false; // ��ֹ�������¿�ʼ
    }

    // ���ƽ���
    private void EndGesture()
    {
        Is_Gesture_InProgress = false;
        gesture_main_con.EndGesture(); // ֪ͨ�����ƽű������ɼ�
        Bool_Gesture_End = false; // ��ֹ�������½���
    }

    // UI��ť�������ƿ�ʼ
    public void On_Gesture_Start()
    {
        if (!Is_Gesture_InProgress) // ��ֹ�ظ���ʼ
        {
            Bool_Gesture_Start = true;
            Bool_Gesture_End = false;
        }
    }

    // UI��ť�������ƽ���
    public void On_Gesture_End()
    {
        if (Is_Gesture_InProgress) // ��ֹû�п�ʼ�ͽ���
        {
            Bool_Gesture_End = true;
            Bool_Gesture_Start = false;
        }
    }
}
