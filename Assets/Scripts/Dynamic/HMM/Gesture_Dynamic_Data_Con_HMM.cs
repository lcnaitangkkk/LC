using UnityEngine;
using Leap;
using System.Collections.Generic; // ����List����

public class Gesture_Dynamic_Data_Con_HMM : MonoBehaviour
{
    public List<float> Palm_Movement_List = new List<float>(); // ����λ���б�
    public List<float> Finger_Extension_List = new List<float>(); // ��ָ��չ���б�
    public List<float> Palm_Angle_List = new List<float>(); // �����ƶ��Ƕ��б�
    public List<float> Movement_Rate_List = new List<float>(); // �˶�����仯���б�

    private Vector3 lastPalmPosition;
    private Vector3 lastHandPosition;

    void Start()
    {
        On_Initialize_GestureData();
    }

    // ��ʼ������
    public void On_Initialize_GestureData()
    {
        Palm_Movement_List.Clear();
        Finger_Extension_List.Clear();
        Palm_Angle_List.Clear();
        Movement_Rate_List.Clear();

        lastPalmPosition = Vector3.zero;
        lastHandPosition = Vector3.zero;
    }

    // �����ɼ���������
    public void On_Collect_Gesture_Data(Hand hand)
    {
        // ��������λ��
        float palmMovement = Vector3.Distance(lastPalmPosition, hand.PalmPosition);
        Palm_Movement_List.Add(palmMovement);
        lastPalmPosition = hand.PalmPosition;

        // ������ָ��չ��
        float fingerExtension = 0f;
        foreach (var finger in hand.fingers)
        {
            if (finger.IsExtended)
            {
                fingerExtension += 1;
            }
        }
        Finger_Extension_List.Add(fingerExtension);

        // ���������ƶ��Ƕ�
        float palmAngle = Vector3.Angle(lastHandPosition - hand.PalmPosition, Vector3.up);
        Palm_Angle_List.Add(palmAngle);
        lastHandPosition = hand.PalmPosition;

        // �����˶�����仯��
        float movementRate = hand.PalmVelocity.magnitude;
        Movement_Rate_List.Add(movementRate);

        // ��鲢���� NaN ֵ
        Palm_Movement_List[Palm_Movement_List.Count - 1] = float.IsNaN(Palm_Movement_List[Palm_Movement_List.Count - 1]) ? 0f : Palm_Movement_List[Palm_Movement_List.Count - 1];
        Finger_Extension_List[Finger_Extension_List.Count - 1] = float.IsNaN(Finger_Extension_List[Finger_Extension_List.Count - 1]) ? 0f : Finger_Extension_List[Finger_Extension_List.Count - 1];
        Palm_Angle_List[Palm_Angle_List.Count - 1] = float.IsNaN(Palm_Angle_List[Palm_Angle_List.Count - 1]) ? 0f : Palm_Angle_List[Palm_Angle_List.Count - 1];
        Movement_Rate_List[Movement_Rate_List.Count - 1] = float.IsNaN(Movement_Rate_List[Movement_Rate_List.Count - 1]) ? 0f : Movement_Rate_List[Movement_Rate_List.Count - 1];
    }

    // ��ȡ����λ���б�
    public List<float> GetPalmMovementList()
    {
        return Palm_Movement_List;
    }

    // ��ȡ��ָ��չ���б�
    public List<float> GetFingerExtensionList()
    {
        return Finger_Extension_List;
    }

    // ��ȡ���ĽǶ��б�
    public List<float> GetPalmAngleList()
    {
        return Palm_Angle_List;
    }

    // ��ȡ�˶�����仯���б�
    public List<float> GetMovementRateList()
    {
        return Movement_Rate_List;
    }
}
