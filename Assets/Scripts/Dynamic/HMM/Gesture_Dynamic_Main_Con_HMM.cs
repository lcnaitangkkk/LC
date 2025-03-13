using UnityEngine;
using Leap;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

public class Gesture_Dynamic_Main_Con_HMM : MonoBehaviour
{
    // ��������ͱ�������
    private Gesture_Dynamic_Start_End_HMM startEndHandler; // ���ڼ�����ƵĿ�ʼ�ͽ���
    private Gesture_Dynamic_Data_Con_HMM dataCollector; // �����ռ���������
    public Gesture_Dynamic_Template_HMM gesture_Template; // ���ڱ���Ͳ�������ģ��
    private Controller leapController; // Leap Motion �����������ڻ�ȡ��������
    private float timer = 0f; // ��ʱ�������ڿ���ÿ��һ��ʱ��ɼ�����
    private const float interval = 0.7f; // ���ò������Ϊ0.7��
    private bool canSendNextFile = true; // �Ƿ���Է�����һ���ļ�
    private bool isRecording = false; // �Ƿ�����¼����������
    public float velocityThreshold = 0.5f; // �ٶ���ֵ�������ж��Ƿ�ʼ¼������
    public Transform targetObject; // Ŀ�����壬������ת
    private SynchronizationContext mainThreadContext; // ���̵߳������ģ����ڸ���UI�Ȳ���
    private int frameCount = 0; // ֡�����������ڿ������ݵĲ���Ƶ��

    // ���������ܵ����ƣ�Python ����˱���һ��
    private const string PIPE_NAME = "GesturePipe"; // �� Python һ��

    // Start �� Unity ���������ڷ������ڽű���ʼ��ʱ����
    void Start()
    {
        // ��ȡ���
        startEndHandler = GetComponent<Gesture_Dynamic_Start_End_HMM>();
        dataCollector = GetComponent<Gesture_Dynamic_Data_Con_HMM>();
        gesture_Template = GetComponent<Gesture_Dynamic_Template_HMM>();

        // ��ʼ�� Leap Motion ������
        leapController = new Controller();

        // ��ȡ���߳������ģ�����������������ת�Ȳ���
        mainThreadContext = SynchronizationContext.Current;
    }

    // Update �� Unity ���������ڷ�������ÿһ֡����ʱ����
    void Update()
    {
        // ��ȡ Leap Motion ��ǰ֡
        Frame frame = leapController.Frame();

        // ������ƵĿ�ʼ�ͽ���
        startEndHandler.On_Check_Gesture_Start(frame);
        startEndHandler.On_Check_Gesture_End(frame);

        // ����������ڽ����У������ֲ�����
        if (startEndHandler.Is_Gesture_InProgress && frame.Hands.Count > 0)
        {
            // ��ȡ��һ���ֲ�����
            Hand hand = frame.Hands[0];
            float palmVelocityMagnitude = hand.PalmVelocity.magnitude; // ��ȡ�����ٶȵĴ�С

            // �����ǰû������¼�����ƣ����������ٶȴ�����ֵ����ʼ¼��
            if (!isRecording && palmVelocityMagnitude > velocityThreshold)
            {
                isRecording = true;
                timer = 0f;
                frameCount = 0;
                dataCollector.On_Initialize_GestureData(); // ��ʼ�����������ռ�
            }

            // �������¼����������
            if (isRecording)
            {
                frameCount++; // ����֡������
                // ÿ����֡�ɼ�һ������
                if (frameCount % 2 == 0)
                {
                    dataCollector.On_Collect_Gesture_Data(hand); // �ռ���������
                }

                timer += Time.deltaTime; // ���Ӽ�ʱ��
                // �����ʱ�������趨�ļ�������ҿ��Է�����һ���ļ�
                if (timer >= interval && canSendNextFile)
                {
                    timer = 0f;
                    isRecording = false; // ����¼��
                    frameCount = 0;
                    canSendNextFile = false; // ��ֹ����������һ���ļ�
                    _ = SaveAndTestTemplate(); // ���沢��������ģ��
                }
            }
        }
        else
        {
            // ������ƽ�����ֹͣ¼��
            isRecording = false;
        }
    }

    // �ֶ��������ƿ�ʼ
    public void StartGesture()
    {
        Debug.Log("Dynamic Gesture Started");
    }

    // �ֶ��������ƽ���
    public void EndGesture()
    {
        Debug.Log("Dynamic Gesture Ended");
        // �������¼�Ʋ��ҿ��Է�����һ���ļ�
        if (isRecording && canSendNextFile)
        {
            isRecording = false;
            frameCount = 0;
            canSendNextFile = false;
            _ = SaveAndTestTemplate(); // ���沢��������ģ��
        }
    }

    // �첽�������沢��������ģ��
    private async Task SaveAndTestTemplate()
    {
        gesture_Template.SaveGestureTemplate(); // ��������ģ��
        await CallPythonServerAsync(); // ���� Python ������в���
    }

    // �첽������ Python �������ͨ��
    private async Task CallPythonServerAsync()
    {
        // ����ģ���ļ�·��
        string filePath = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem_Test/MyDynamicGesture.json";

        // ���������ܵ��ͻ��ˣ����ӵ� Python �˵Ĺܵ�
        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut))
        {
            try
            {
                // �첽���ӵ��ܵ�
                await pipeClient.ConnectAsync(3000);
                byte[] requestBytes = Encoding.UTF8.GetBytes(filePath); // ���ļ�·��תΪ�ֽ�����
                await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length); // д����������
                await pipeClient.FlushAsync(); // ˢ�¹ܵ�����

                byte[] responseBytes = new byte[256]; // �����ֽ��������ڽ�����Ӧ
                int bytesRead = await pipeClient.ReadAsync(responseBytes, 0, responseBytes.Length); // ��ȡ��Ӧ����
                string result = Encoding.UTF8.GetString(responseBytes, 0, bytesRead).Trim(); // ���ֽ�����ת��Ϊ�ַ���

                Debug.Log($"Python Response: {result}"); // ��� Python ����Ӧ

                // ��� Python ���� "ZuoToYou"������תĿ������
                if (result == "ZuoToYou")
                {
                    mainThreadContext.Post(_ => RotateObject(), null); // �����߳��е�����ת����ķ���
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"Pipe communication error: {ex.Message}"); // ����ܵ�ͨ���쳣
            }
        }

        // ��������һ���ļ�
        canSendNextFile = true;
    }

    // ��תĿ������
    private void RotateObject()
    {
        // ���Ŀ�����岻Ϊ�գ��������ת
        if (targetObject != null)
        {
            targetObject.Rotate(0f, 30f, 0f, Space.Self); // ��Ŀ������ľֲ�����ϵ��ת
        }
    }
}
