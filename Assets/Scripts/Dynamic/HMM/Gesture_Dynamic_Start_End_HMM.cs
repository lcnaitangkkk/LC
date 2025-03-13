using UnityEngine;
using Leap;
using UnityEngine.UI;

public class Gesture_Dynamic_Start_End_HMM : MonoBehaviour
{
    public bool Is_Gesture_InProgress = false; // �ж������Ƿ����ڽ���
    public float Gesture_End_Time;
    public float Gesture_Hold_Time = 1f; // ���Ʊ���ʱ����ֵ
    public float Gesture_Max_Duration = 2f; // ����¼���Ƶ�ʱ��
    private float Gesture_Start_Time; // ���ƿ�ʼ��ʱ��
    public Gesture_Dynamic_Main_Con_HMM gesture_main_con;
    public Button Gesture_Start;
    public Button Gesture_End;
    public bool Bool_Gesture_Start = false;
    public bool Bool_Gesture_End = false;
    private Vector3 lastPalmPosition; // �洢��һ֡���Ƶ�λ��
    private float lastPalmVelocityMagnitude = 0f; // �����ٶȵĴ�С

    void Start()
    {
        gesture_main_con = GetComponent<Gesture_Dynamic_Main_Con_HMM>();
        lastPalmPosition = Vector3.zero;

        // �� UI ��ť�¼�
        Gesture_Start.onClick.AddListener(On_Gesture_Start);
        Gesture_End.onClick.AddListener(On_Gesture_End);
    }

    // ��������Ƿ�ʼ
    public void On_Check_Gesture_Start(Frame frame)
    {
        if (!Is_Gesture_InProgress && frame.Hands.Count > 0 && Bool_Gesture_Start == true)
        {
            Hand hand = frame.Hands[0]; // ѡ���һֻ��

            // �ж������Ƿ�����Ч��Χ���Ҳ��ھ�ֹ״̬
            if (hand.PalmPosition.magnitude > 0f)
            {
                StartGesture();
            }

            lastPalmPosition = hand.PalmPosition;
        }
    }

    // ��������Ƿ����
    public void On_Check_Gesture_End(Frame frame)
    {
        if (Is_Gesture_InProgress)
        {
            // ����Ƿ񳬹�����¼ʱ��
            if (Time.time - Gesture_Start_Time >= Gesture_Max_Duration)
            {
                //EndGesture();
                return;
            }

            if (Bool_Gesture_End == true)
            {
                EndGesture();
            }
            else if (frame.Hands.Count > 0)
            {
                Hand hand = frame.Hands[0];

                // ��ȡ�����ٶȵĴ�С
                float palmVelocityMagnitude = hand.PalmVelocity.magnitude;

                // ����������ƶ��������ý���ʱ��
                if (palmVelocityMagnitude > 0.1f) // ����һ����С�ٶ���ֵ����ֹ����
                {
                    Gesture_End_Time = 0f;
                }
                else
                {
                    // ����ٶ�С����ֵ����ʼ��ʱ
                    Gesture_End_Time += Time.deltaTime;
                }

                // �����ֹʱ�䳬����ֵ�����������
                if (Gesture_End_Time >= Gesture_Hold_Time)
                {
                    //EndGesture();
                }

                lastPalmPosition = hand.PalmPosition;
            }
        }
    }

    // ���ƿ�ʼ
    private void StartGesture()
    {
        Is_Gesture_InProgress = true;
        Gesture_Start_Time = Time.time; // ��¼���ƿ�ʼʱ��
        Bool_Gesture_Start = false; // ��ֹ�ظ���ʼ
    }

    // ���ƽ���
    private void EndGesture()
    {
        Is_Gesture_InProgress = false;
        gesture_main_con.EndGesture(); // ֪ͨ��������������¼
        Bool_Gesture_End = false; // ��ֹ�ظ�����
    }

    // UI ��ť����¼������ƿ�ʼ
    public void On_Gesture_Start()
    {
        if (!Is_Gesture_InProgress) // ��ֹ�ظ���ʼ
        {
            Bool_Gesture_Start = true;
            Bool_Gesture_End = false;
        }
    }

    // UI ��ť����¼������ƽ���
    public void On_Gesture_End()
    {
        if (Is_Gesture_InProgress) // ��ֹû��ʼ�ͽ���
        {
            Bool_Gesture_End = true;
            Bool_Gesture_Start = false;
        }
    }
}