using Leap;
using System.Numerics;
using UnityEngine;

public class Gesture_KNN_Main_Con : MonoBehaviour
{
    private Gesture_KNN_Recognizer knnRecognizer;
    public Gesture_Data_Con dataCollector1;
    public GameObject cube;
    public GameObject cylinder;
    public GameObject sphere;
    public GameObject plane;
    private CarMovement carMovement;
    private string currentGesture = "";  // ��ǰ����ִ�е�����
    private bool isGestureRecognized = false; // ��ǰ�����Ƿ�ʶ��

    void Start()
    {

        knnRecognizer = GetComponent<Gesture_KNN_Recognizer>();
        dataCollector1 = GetComponent<Gesture_Data_Con>();
        GameObject car = GameObject.Find("car");
        carMovement = car.GetComponent<CarMovement>();
        // �����������ݼ������ﴫ��洢����ģ����ļ���·����
        knnRecognizer.LoadGestureDataset(Application.dataPath + "/Gesture_Tem");
    }

    void Update()
    {
        // ��ȡ��ǰ֡����������
        Frame frame = new Controller().Frame();

        // ����Ƿ����ִ���
        if (frame.Hands.Count == 0)
        {
            Debug.Log("û�м�⵽��");
            HandleGestureEnd();  // ���ƽ���ʱ����
            return; // û���֣�����ʶ�����
        }

        // ʹ�� Gesture_Data_Con �ռ���ǰ֡������
        Hand hand = frame.Hands[0];  // ѡ���һ����⵽����
        dataCollector1.On_Collect_Gesture_Data(hand); // �ռ���������

        // ��ȡ��ǰ���Ƶ���������ָ��չ�ȡ���ָ�Ƕȡ���ָ�����ĵľ��롢��ָ���໥֮��ľ���
        float fingerExtension = dataCollector1.GetFingerExtension();
        float fingertipsAngle = dataCollector1.GetFingertipsAngle();
        float fingertipsDistance = dataCollector1.GetFingertipsDistance();
        float fingertipsMutualDistance = dataCollector1.GetFingertipsMutualDistance();

        // ʶ��ǰ����
        string recognizedGesture = knnRecognizer.RecognizeGesture(fingerExtension, fingertipsAngle, fingertipsDistance, fingertipsMutualDistance);

        // ���ʶ�𵽵����Ʒ����仯
        if (recognizedGesture != currentGesture)
        {
            if (currentGesture != "") // �����ǰ���Ʋ�Ϊ�գ���ζ������������ִ�У����ý������ƺ���
            {
                HandleGestureEnd();  // ������ǰ����
            }

            HandleGestureStart(recognizedGesture); // ��ʼ�µ�����
        }
    }

    // �������ƿ�ʼ���߼�
    private void HandleGestureStart(string recognizedGesture)
    {
        currentGesture = recognizedGesture;
        isGestureRecognized = true;

        // ����ʶ�������ִ�в�ͬ�Ŀ�ʼ����
        if (recognizedGesture == "YE")
        {
            plane.GetComponent<Renderer>().material.color = Color.blue;
            Debug.Log("��ʼִ�� YE ����");
            // ִ�� YE ���ƿ�ʼʱ�Ĳ���j
        }
        else if (recognizedGesture == "fist")
        {
            Debug.Log("��ʼִ�� fist ����");
            cube.GetComponent<Renderer>().material.color = Color.yellow;
            carMovement.speed = 0;
            // ִ�� fist ���ƿ�ʼʱ�Ĳ���
        }
        else if (recognizedGesture == "zhang")
        {
            Debug.Log("��ʼִ�� Zhang ����");
            cylinder.GetComponent<Renderer>().material.color = Color.green;
            carMovement.speed = 2;
            // ִ�� Zhang ���ƿ�ʼʱ�Ĳ���
        }
        else if (recognizedGesture == "rock")
        {
            Debug.Log("��ʼִ�� Rock ����");
            sphere.GetComponent<Renderer>().material.color = Color.red;
            // ִ�� Rock ���ƿ�ʼʱ�Ĳ���
        }
    }

    // �������ƽ������߼�
    private void HandleGestureEnd()
    {
        if (isGestureRecognized)
        {
            Debug.Log("����ִ�� " + currentGesture + " ����");
            // ִ�����ƽ���ʱ�Ĳ���
            // ����ָ�����״̬��ֹͣĳЩ��Ϊ

            // ����״̬
            isGestureRecognized = false;

            // ���Ը��ݵ�ǰ���ƽ�����Ӧ�Ľ�������
            if (currentGesture == "YE")
            {
                plane.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("���� YE ����");
                // ִ�� YE ���ƽ���ʱ�Ĳ���
            }
            else if (currentGesture == "fist")
            {
                cube.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("���� fist ����");
                // ִ�� fist ���ƽ���ʱ�Ĳ���
            }
            else if (currentGesture == "zhang")
            {
                cylinder.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("���� Zhang ����");
                // ִ�� Zhang ���ƽ���ʱ�Ĳ���
            }
            else if (currentGesture == "rock")
            {
                sphere.GetComponent<Renderer>().material.color = Color.white;
                Debug.Log("���� Rock ����");
                // ִ�� Rock ���ƽ���ʱ�Ĳ���
            }

            currentGesture = ""; // ��յ�ǰ����
        }
    }
}
