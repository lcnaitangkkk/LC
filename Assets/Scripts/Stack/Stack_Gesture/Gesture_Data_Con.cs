using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class Gesture_Data_Con : MonoBehaviour
{
    public float Finger_Extension; // 手指伸展度
    public float Fingertips_Angle; // 手指角度特征
    public float Fingertips_Distance; // 手指与掌心的距离特征
    public float Fingertips_Mutual_Distance; // 手指尖相互距离特征

    // 用于计算手指角度和手指尖之间的距离
    private List<Vector3> fingertipPositions;

    void Start()
    {
        On_Initialize_GestureData();
    }

    // 初始化数据
    public void On_Initialize_GestureData()
    {
        Finger_Extension = 0f;
        Fingertips_Angle = 0f;
        Fingertips_Distance = 0f;
        Fingertips_Mutual_Distance = 0f;
        fingertipPositions = new List<Vector3>();
    }

    // 继续采集手势数据
    public void On_Collect_Gesture_Data(Hand hand)
    {
        // 初始化数据
        Finger_Extension = 0f;
        Fingertips_Angle = 0f;
        Fingertips_Distance = 0f;
        Fingertips_Mutual_Distance = 0f;

        fingertipPositions.Clear();

        // 计算手指伸展度和记录手指尖的位置
        foreach (var finger in hand.fingers)
        {
            if (finger.IsExtended)
            {
                Finger_Extension += 1; // 手指伸展的数量
                fingertipPositions.Add(finger.TipPosition); // 记录手指尖的位置
            }
        }

        // 计算手指角度特征（手指尖之间的夹角）
        if (fingertipPositions.Count >= 2)
        {
            Fingertips_Angle = CalculateFingertipsAngle(fingertipPositions[0], fingertipPositions[1], hand.PalmPosition);
        }

        // 计算手指与掌心的距离特征
        Fingertips_Distance = CalculateFingertipsDistance(hand.PalmPosition, fingertipPositions);

        // 计算手指尖相互之间的距离
        Fingertips_Mutual_Distance = CalculateFingertipsMutualDistance(fingertipPositions);

        // 检查并处理 NaN 值
        Finger_Extension = float.IsNaN(Finger_Extension) ? 0f : Finger_Extension;
        Fingertips_Angle = float.IsNaN(Fingertips_Angle) ? 0f : Fingertips_Angle;
        Fingertips_Distance = float.IsNaN(Fingertips_Distance) ? 0f : Fingertips_Distance;
        Fingertips_Mutual_Distance = float.IsNaN(Fingertips_Mutual_Distance) ? 0f : Fingertips_Mutual_Distance;
    }

    // 计算手指角度（两个手指尖之间的夹角）
    private float CalculateFingertipsAngle(Vector3 fingertip1, Vector3 fingertip2, Vector3 palmPosition)
    {
        // 计算从掌心到每个手指尖的方向向量
        Vector3 direction1 = fingertip1 - palmPosition; // 手指1与掌心的方向
        Vector3 direction2 = fingertip2 - palmPosition; // 手指2与掌心的方向

        // 计算两个方向的夹角
        return Vector3.Angle(direction1, direction2);
    }

    // 计算手指尖和掌心的距离
    private float CalculateFingertipsDistance(Vector3 palmPosition, List<Vector3> fingertipPositions)
    {
        float totalDistance = 0f;

        foreach (var fingertip in fingertipPositions)
        {
            totalDistance += Vector3.Distance(palmPosition, fingertip);
        }

        return fingertipPositions.Count > 0 ? totalDistance / fingertipPositions.Count : 0f; // 平均距离
    }

    // 计算手指尖之间的相互距离
    private float CalculateFingertipsMutualDistance(List<Vector3> fingertipPositions)
    {
        float totalDistance = 0f;

        for (int i = 0; i < fingertipPositions.Count; i++)
        {
            for (int j = i + 1; j < fingertipPositions.Count; j++)
            {
                totalDistance += Vector3.Distance(fingertipPositions[i], fingertipPositions[j]);
            }
        }

        int numDistances = fingertipPositions.Count * (fingertipPositions.Count - 1) / 2;
        return numDistances > 0 ? totalDistance / numDistances : 0f; // 平均距离
    }

    // 获取手指伸展度
    public float GetFingerExtension()
    {
        return Finger_Extension;
    }

    // 获取手指角度特征
    public float GetFingertipsAngle()
    {
        return Fingertips_Angle;
    }

    // 获取手指与掌心的距离特征
    public float GetFingertipsDistance()
    {
        return Fingertips_Distance;
    }

    // 获取手指尖相互之间的距离特征
    public float GetFingertipsMutualDistance()
    {
        return Fingertips_Mutual_Distance;
    }
}
