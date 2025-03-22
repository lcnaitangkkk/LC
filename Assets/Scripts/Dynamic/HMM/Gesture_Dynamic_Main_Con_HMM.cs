using UnityEngine;
using Leap;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Collections.Generic;

public class Gesture_Dynamic_Main_Con_HMM : MonoBehaviour
{
    // ����������
    private Gesture_Dynamic_Start_End_HMM startEndHandler; // ���ڼ�����ƵĿ�ʼ�ͽ���
    private Gesture_Dynamic_Data_Con_HMM dataCollector;      // ���ڲɼ���������
    public Gesture_Dynamic_Template_HMM gesture_Template;    // ���ڱ���ͼ�������ģ��
    private Controller leapController;                      // Leap Motion �����������ڻ�ȡ�ֲ�����
    private float timer = 0f;                               // ��ʱ�������ڿ������ݲɼ����
    private const float interval = 0.7f;                    // ���ݲɼ�ʱ������0.7�룩
    private bool canSendNextFile = true;                    // �Ƿ���Է�����һ���ļ��������ظ�����
    private bool isRecording = false;                       // �Ƿ�����¼����������
    public float velocityThreshold = 0.5f;                  // �����ٶ���ֵ�������ж��Ƿ�ʼ¼������
    public float fingerVelocityThreshold = 0.5f;            // ��ָ�ٶ���ֵ�������ж���ָ�Ƿ��ƶ�
    public float transmissionDelay = 2.0f;                  // ���ݴ����ӳ�ʱ�䣨�룩
    public Transform targetObject;                        // Ŀ�����������ת�Ȳ���
    private SynchronizationContext mainThreadContext;     // ���߳������ģ����ڸ���UI�Ȳ���
    private int frameCount = 0;                           // ֡�������������ڿ������ݲɼ�Ƶ��
    public HMM_CAR hMM_CAR;

    // ���ڼ�¼ÿ����ָ����һ֡λ�ã�key Ϊ��ָ ID
    private Dictionary<int, Vector3> previousFingerPositions = new Dictionary<int, Vector3>();

    // �� Python ������ͨ�ŵĹܵ�����
    private const string PIPE_NAME = "GesturePipe";       // �� Python һ��

    void Start()
    {
        // ��ȡ���
        startEndHandler = GetComponent<Gesture_Dynamic_Start_End_HMM>();
        dataCollector = GetComponent<Gesture_Dynamic_Data_Con_HMM>();
        gesture_Template = GetComponent<Gesture_Dynamic_Template_HMM>();

        // ��ʼ�� Leap Motion ������
        leapController = new Controller();

        // ��ȡ���߳�������
        mainThreadContext = SynchronizationContext.Current;
    }

    void Update()
    {
        // ��ȡ��ǰ Leap Motion ֡
        Frame frame = leapController.Frame();

        // ������ƵĿ�ʼ�ͽ���
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // ����������ڽ��������ֲ�����
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            Hand hand = frame.Hands[0];
            float palmVelocityMagnitude = hand.PalmVelocity.magnitude; // ��ȡ�����ٶ�

            // �ж��Ƿ�����ָ�ƶ�
            bool fingerMoving = IsAnyFingerMoving(hand);

            // ֻ�е� canSendNextFile Ϊ true ʱ���ſ�ʼ�µ�¼��
            if (!isRecording && canSendNextFile && (palmVelocityMagnitude > velocityThreshold || fingerMoving))
            {
                isRecording = true;
                timer = 0f;
                frameCount = 0;
                dataCollector.On_Initialize_GestureData();
            }

            if (isRecording)
            {
                frameCount++; // ֡����1
                // ÿ��֡�ɼ�һ������
                if (frameCount % 2 == 0)
                {
                    dataCollector.On_Collect_Gesture_Data(hand); // �ɼ���������
                }

                timer += Time.deltaTime;
                // �ﵽʱ�����󣬱������ݲ����͵� Python ������
                if (timer >= interval && canSendNextFile)
                {
                    timer = 0f;
                    isRecording = false;
                    frameCount = 0;
                    canSendNextFile = false;  // ���ͺ���ͣ¼�ƣ��ȴ���Ӧ
                    _ = SaveAndTestTemplate();
                }
            }
        }
        else
        {
            // ���ƽ�����ֹͣ¼��
            isRecording = false;
        }
    }

    // �ж��Ƿ�����ָ�ƶ�
    private bool IsAnyFingerMoving(Hand hand)
    {
        bool isMoving = false;
        foreach (Finger finger in hand.fingers)
        {
            Vector3 currentPos = new Vector3(finger.TipPosition.x, finger.TipPosition.y, finger.TipPosition.z);
            if (previousFingerPositions.TryGetValue(finger.Id, out Vector3 prevPos))
            {
                float fingerSpeed = (currentPos - prevPos).magnitude / Time.deltaTime;
                if (fingerSpeed > fingerVelocityThreshold)
                {
                    isMoving = true;
                }
                previousFingerPositions[finger.Id] = currentPos;
            }
            else
            {
                previousFingerPositions[finger.Id] = currentPos;
            }
        }
        return isMoving;
    }

    public void StartGesture()
    {
        Debug.Log("Dynamic Gesture Started");
    }

    public void EndGesture()
    {
        Debug.Log("Dynamic Gesture Ended");
        if (isRecording && canSendNextFile)
        {
            isRecording = false;
            frameCount = 0;
            canSendNextFile = false;
            _ = SaveAndTestTemplate();
        }
    }

    // ��������ģ�岢���Ե��첽����
    private async Task SaveAndTestTemplate()
    {
        gesture_Template.SaveGestureTemplate();
        await CallPythonServerAsync();
    }

    // �� Python ������ͨ�ŵ��첽����
    private async Task CallPythonServerAsync()
    {
        string filePath = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem_Test/MyDynamicGesture.json";

        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut))
        {
            try
            {
                await pipeClient.ConnectAsync(3000);
                byte[] requestBytes = Encoding.UTF8.GetBytes(filePath);
                await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);
                await pipeClient.FlushAsync();

                byte[] responseBytes = new byte[256];
                int bytesRead = await pipeClient.ReadAsync(responseBytes, 0, responseBytes.Length);
                string result = Encoding.UTF8.GetString(responseBytes, 0, bytesRead).Trim();

                Debug.Log($"Python Response: {result}");
                if (result == "ZuoToYou")
                {
                    mainThreadContext.Post(_ => hMM_CAR.Car_Rotate(), null);
                }
                else if (result == "FangDa")
                {
                    mainThreadContext.Post(_ => hMM_CAR.Camera_Near(), null);
                }
                else if (result == "XiaToShang")
                {
                    mainThreadContext.Post(_ => hMM_CAR.Car_Jump(), null);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Pipe communication error: {ex.Message}");
            }
        }

        // �ȴ������ӳ�ʱ�䣬׼����һ��¼��
        await Task.Delay((int)(transmissionDelay * 1000));
        canSendNextFile = true;
    }
}