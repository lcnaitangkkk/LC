using UnityEngine;
using Leap;

public class Gesture_Dynamic_Main_Con : MonoBehaviour
{
    private Gesture_Dynamic_Start_End startEndHandler;
    private Gesture_Dynamic_Data_Con dataCollector;
    public Gesture_Dynamic_Template gesture_Template;
    private Controller leapController; // ֻ����һ�� Leap Controller
    private int frameCounter = 0; // ֡��������

    void Start()
    {
        startEndHandler = GetComponent<Gesture_Dynamic_Start_End>();
        dataCollector = GetComponent<Gesture_Dynamic_Data_Con>();
        gesture_Template = GetComponent<Gesture_Dynamic_Template>();

        leapController = new Controller(); // �� Start �д���һ�� Controller
    }

    void Update()
    {
        Frame frame = leapController.Frame(); // ʹ���Ѵ����� Controller ʵ����ȡ֡����

        // ������ƿ�ʼ�ͽ���
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // �ռ���������
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            frameCounter++;
            if (frameCounter % 2 == 0) // ÿ��֡��¼һ������
            {
                Hand hand = frame.Hands[0]; // ��ȡ��һֻ�ֵ�����
                dataCollector.On_Collect_Gesture_Data(hand); // �ռ���������
            }
        }
    }

    // ���ƿ�ʼ
    public void StartGesture()
    {
        dataCollector.On_Initialize_GestureData(); // ��ʼ����������
        frameCounter = 0; // ����֡��������
        Debug.Log("Dynamic Gesture Started");
    }

    // ���ƽ���
    public void EndGesture()
    {
        Debug.Log("Dynamic Gesture Ended");

        // ��������ģ��
        // ȷ�������Ѿ���ȫ�ռ����ڱ���֮ǰ��Ҫ���б�Ҫ����֤
        gesture_Template.SaveGestureTemplate(); // ��������ģ��
    }
}