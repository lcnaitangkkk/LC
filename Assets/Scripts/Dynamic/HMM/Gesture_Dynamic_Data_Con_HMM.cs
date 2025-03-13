using UnityEngine;
using Leap;
using System.Collections.Generic; // 引入List类型

public class Gesture_Dynamic_Data_Con_HMM : MonoBehaviour
{
    public List<float> Palm_Movement_List = new List<float>(); // 掌心位移列表
    public List<float> Finger_Extension_List = new List<float>(); // 手指伸展度列表
    public List<float> Palm_Angle_List = new List<float>(); // 掌心移动角度列表
    public List<float> Movement_Rate_List = new List<float>(); // 运动方向变化率列表

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
        Palm_Angle_List.Clear();
        Movement_Rate_List.Clear();

        lastPalmPosition = Vector3.zero;
        lastHandPosition = Vector3.zero;
    }

    // 继续采集手势数据
    public void On_Collect_Gesture_Data(Hand hand)
    {
        // 计算掌心位移
        float palmMovement = Vector3.Distance(lastPalmPosition, hand.PalmPosition);
        Palm_Movement_List.Add(palmMovement);
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

        // 计算掌心移动角度
        float palmAngle = Vector3.Angle(lastHandPosition - hand.PalmPosition, Vector3.up);
        Palm_Angle_List.Add(palmAngle);
        lastHandPosition = hand.PalmPosition;

        // 计算运动方向变化率
        float movementRate = hand.PalmVelocity.magnitude;
        Movement_Rate_List.Add(movementRate);

        // 检查并处理 NaN 值
        Palm_Movement_List[Palm_Movement_List.Count - 1] = float.IsNaN(Palm_Movement_List[Palm_Movement_List.Count - 1]) ? 0f : Palm_Movement_List[Palm_Movement_List.Count - 1];
        Finger_Extension_List[Finger_Extension_List.Count - 1] = float.IsNaN(Finger_Extension_List[Finger_Extension_List.Count - 1]) ? 0f : Finger_Extension_List[Finger_Extension_List.Count - 1];
        Palm_Angle_List[Palm_Angle_List.Count - 1] = float.IsNaN(Palm_Angle_List[Palm_Angle_List.Count - 1]) ? 0f : Palm_Angle_List[Palm_Angle_List.Count - 1];
        Movement_Rate_List[Movement_Rate_List.Count - 1] = float.IsNaN(Movement_Rate_List[Movement_Rate_List.Count - 1]) ? 0f : Movement_Rate_List[Movement_Rate_List.Count - 1];
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

    // 获取掌心角度列表
    public List<float> GetPalmAngleList()
    {
        return Palm_Angle_List;
    }

    // 获取运动方向变化率列表
    public List<float> GetMovementRateList()
    {
        return Movement_Rate_List;
    }
}
