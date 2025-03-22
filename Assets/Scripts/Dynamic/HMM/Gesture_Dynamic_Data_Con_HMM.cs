using UnityEngine;
using Leap;
using System.Collections.Generic; // ����List����

public class Gesture_Dynamic_Data_Con_HMM : MonoBehaviour
{
    public List<float> Palm_Movement_List = new List<float>(); // ����λ���б�
    public List<float> Finger_Extension_List = new List<float>(); // ��ָ��չ���б�
    public List<float> Movement_Rate_List = new List<float>(); // �˶�����仯���б�
    public List<Vector3> Palm_Movement_Direction_List = new List<Vector3>(); // �����ƶ������б�
    public List<Quaternion> Palm_Rotation_Quaternion_List = new List<Quaternion>(); // ������ת��Ԫ���б�

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
        Movement_Rate_List.Clear();
        Palm_Movement_Direction_List.Clear();
        Palm_Rotation_Quaternion_List.Clear();

        lastPalmPosition = Vector3.zero;
        lastHandPosition = Vector3.zero;
    }

    // �����ɼ���������
    public void On_Collect_Gesture_Data(Hand hand)
    {
        // ��������λ��
        float palmMovement = Vector3.Distance(lastPalmPosition, hand.PalmPosition);
        Palm_Movement_List.Add(palmMovement);

        // ���������ƶ�����
        Vector3 palmMovementDirection = (hand.PalmPosition - lastPalmPosition).normalized;
        Palm_Movement_Direction_List.Add(palmMovementDirection);

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

        // �����˶�����仯��
        float movementRate = hand.PalmVelocity.magnitude;
        Movement_Rate_List.Add(movementRate);

        // ����������ת��Ԫ��
        Quaternion rotation = hand.Rotation;
        Palm_Rotation_Quaternion_List.Add(rotation);

        // ��鲢���� NaN ֵ
        Palm_Movement_List[Palm_Movement_List.Count - 1] = float.IsNaN(Palm_Movement_List[Palm_Movement_List.Count - 1]) ? 0f : Palm_Movement_List[Palm_Movement_List.Count - 1];
        Finger_Extension_List[Finger_Extension_List.Count - 1] = float.IsNaN(Finger_Extension_List[Finger_Extension_List.Count - 1]) ? 0f : Finger_Extension_List[Finger_Extension_List.Count - 1];
        Movement_Rate_List[Movement_Rate_List.Count - 1] = float.IsNaN(Movement_Rate_List[Movement_Rate_List.Count - 1]) ? 0f : Movement_Rate_List[Movement_Rate_List.Count - 1];
        if (float.IsNaN(Palm_Movement_Direction_List[Palm_Movement_Direction_List.Count - 1].x) ||
            float.IsNaN(Palm_Movement_Direction_List[Palm_Movement_Direction_List.Count - 1].y) ||
            float.IsNaN(Palm_Movement_Direction_List[Palm_Movement_Direction_List.Count - 1].z))
        {
            Palm_Movement_Direction_List[Palm_Movement_Direction_List.Count - 1] = Vector3.zero;
        }
        if (float.IsNaN(Palm_Rotation_Quaternion_List[Palm_Rotation_Quaternion_List.Count - 1].x) ||
            float.IsNaN(Palm_Rotation_Quaternion_List[Palm_Rotation_Quaternion_List.Count - 1].y) ||
            float.IsNaN(Palm_Rotation_Quaternion_List[Palm_Rotation_Quaternion_List.Count - 1].z) ||
            float.IsNaN(Palm_Rotation_Quaternion_List[Palm_Rotation_Quaternion_List.Count - 1].w))
        {
            Palm_Rotation_Quaternion_List[Palm_Rotation_Quaternion_List.Count - 1] = Quaternion.identity;
        }
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

    // ��ȡ�˶�����仯���б�
    public List<float> GetMovementRateList()
    {
        return Movement_Rate_List;
    }

    // ��ȡ�����ƶ������б�
    public List<Vector3> GetPalmMovementDirectionList()
    {
        return Palm_Movement_Direction_List;
    }

    // ��ȡ������ת��Ԫ���б�
    public List<Quaternion> GetPalmRotationQuaternionList()
    {
        return Palm_Rotation_Quaternion_List;
    }
}