using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class Gesture_Data_Con : MonoBehaviour
{
    public float Finger_Extension; // ��ָ��չ��
    public float Fingertips_Angle; // ��ָ�Ƕ�����
    public float Fingertips_Distance; // ��ָ�����ĵľ�������
    public float Fingertips_Mutual_Distance; // ��ָ���໥��������

    // ���ڼ�����ָ�ǶȺ���ָ��֮��ľ���
    private List<Vector3> fingertipPositions;

    void Start()
    {
        On_Initialize_GestureData();
    }

    // ��ʼ������
    public void On_Initialize_GestureData()
    {
        Finger_Extension = 0f;
        Fingertips_Angle = 0f;
        Fingertips_Distance = 0f;
        Fingertips_Mutual_Distance = 0f;
        fingertipPositions = new List<Vector3>();
    }

    // �����ɼ���������
    public void On_Collect_Gesture_Data(Hand hand)
    {
        // ��ʼ������
        Finger_Extension = 0f;
        Fingertips_Angle = 0f;
        Fingertips_Distance = 0f;
        Fingertips_Mutual_Distance = 0f;

        fingertipPositions.Clear();

        // ������ָ��չ�Ⱥͼ�¼��ָ���λ��
        foreach (var finger in hand.fingers)
        {
            if (finger.IsExtended)
            {
                Finger_Extension += 1; // ��ָ��չ������
                fingertipPositions.Add(finger.TipPosition); // ��¼��ָ���λ��
            }
        }

        // ������ָ�Ƕ���������ָ��֮��ļнǣ�
        if (fingertipPositions.Count >= 2)
        {
            Fingertips_Angle = CalculateFingertipsAngle(fingertipPositions[0], fingertipPositions[1], hand.PalmPosition);
        }

        // ������ָ�����ĵľ�������
        Fingertips_Distance = CalculateFingertipsDistance(hand.PalmPosition, fingertipPositions);

        // ������ָ���໥֮��ľ���
        Fingertips_Mutual_Distance = CalculateFingertipsMutualDistance(fingertipPositions);

        // ��鲢���� NaN ֵ
        Finger_Extension = float.IsNaN(Finger_Extension) ? 0f : Finger_Extension;
        Fingertips_Angle = float.IsNaN(Fingertips_Angle) ? 0f : Fingertips_Angle;
        Fingertips_Distance = float.IsNaN(Fingertips_Distance) ? 0f : Fingertips_Distance;
        Fingertips_Mutual_Distance = float.IsNaN(Fingertips_Mutual_Distance) ? 0f : Fingertips_Mutual_Distance;
    }

    // ������ָ�Ƕȣ�������ָ��֮��ļнǣ�
    private float CalculateFingertipsAngle(Vector3 fingertip1, Vector3 fingertip2, Vector3 palmPosition)
    {
        // ��������ĵ�ÿ����ָ��ķ�������
        Vector3 direction1 = fingertip1 - palmPosition; // ��ָ1�����ĵķ���
        Vector3 direction2 = fingertip2 - palmPosition; // ��ָ2�����ĵķ���

        // ������������ļн�
        return Vector3.Angle(direction1, direction2);
    }

    // ������ָ������ĵľ���
    private float CalculateFingertipsDistance(Vector3 palmPosition, List<Vector3> fingertipPositions)
    {
        float totalDistance = 0f;

        foreach (var fingertip in fingertipPositions)
        {
            totalDistance += Vector3.Distance(palmPosition, fingertip);
        }

        return fingertipPositions.Count > 0 ? totalDistance / fingertipPositions.Count : 0f; // ƽ������
    }

    // ������ָ��֮����໥����
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
        return numDistances > 0 ? totalDistance / numDistances : 0f; // ƽ������
    }

    // ��ȡ��ָ��չ��
    public float GetFingerExtension()
    {
        return Finger_Extension;
    }

    // ��ȡ��ָ�Ƕ�����
    public float GetFingertipsAngle()
    {
        return Fingertips_Angle;
    }

    // ��ȡ��ָ�����ĵľ�������
    public float GetFingertipsDistance()
    {
        return Fingertips_Distance;
    }

    // ��ȡ��ָ���໥֮��ľ�������
    public float GetFingertipsMutualDistance()
    {
        return Fingertips_Mutual_Distance;
    }
}
