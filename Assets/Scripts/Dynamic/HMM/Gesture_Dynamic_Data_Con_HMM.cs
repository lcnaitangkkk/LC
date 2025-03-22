using UnityEngine;
using Leap;
using System.Collections.Generic; // 引入List类型

public class Gesture_Dynamic_Data_Con_HMM : MonoBehaviour
{
    public List<float> Palm_Movement_List = new List<float>(); // 掌心位移列表
    public List<float> Finger_Extension_List = new List<float>(); // 手指伸展度列表
    public List<float> Movement_Rate_List = new List<float>(); // 运动方向变化率列表
    public List<Vector3> Palm_Movement_Direction_List = new List<Vector3>(); // 手掌移动方向列表
    public List<Quaternion> Palm_Rotation_Quaternion_List = new List<Quaternion>(); // 手掌旋转四元数列表

    private Vector3 lastPalmPosition;
    private Vector3 lastHandPosition;

    void Start()
    {
        On_Initialize_GestureData();
    }

    // 初始化数据
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

    // 继续采集手势数据
    public void On_Collect_Gesture_Data(Hand hand)
    {
        // 计算掌心位移
        float palmMovement = Vector3.Distance(lastPalmPosition, hand.PalmPosition);
        Palm_Movement_List.Add(palmMovement);

        // 计算手掌移动方向
        Vector3 palmMovementDirection = (hand.PalmPosition - lastPalmPosition).normalized;
        Palm_Movement_Direction_List.Add(palmMovementDirection);

        lastPalmPosition = hand.PalmPosition;

        // 计算手指伸展度
        float fingerExtension = 0f;
        foreach (var finger in hand.fingers)
        {
            if (finger.IsExtended)
            {
                fingerExtension += 1;
            }
        }
        Finger_Extension_List.Add(fingerExtension);

        // 计算运动方向变化率
        float movementRate = hand.PalmVelocity.magnitude;
        Movement_Rate_List.Add(movementRate);

        // 计算手掌旋转四元数
        Quaternion rotation = hand.Rotation;
        Palm_Rotation_Quaternion_List.Add(rotation);

        // 检查并处理 NaN 值
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

    // 获取掌心位移列表
    public List<float> GetPalmMovementList()
    {
        return Palm_Movement_List;
    }

    // 获取手指伸展度列表
    public List<float> GetFingerExtensionList()
    {
        return Finger_Extension_List;
    }

    // 获取运动方向变化率列表
    public List<float> GetMovementRateList()
    {
        return Movement_Rate_List;
    }

    // 获取手掌移动方向列表
    public List<Vector3> GetPalmMovementDirectionList()
    {
        return Palm_Movement_Direction_List;
    }

    // 获取手掌旋转四元数列表
    public List<Quaternion> GetPalmRotationQuaternionList()
    {
        return Palm_Rotation_Quaternion_List;
    }
}