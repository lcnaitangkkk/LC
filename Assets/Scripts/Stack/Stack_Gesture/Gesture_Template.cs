using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Gesture_Template : MonoBehaviour
{
    public Gesture_Data_Con gesture_Data_Con;
    public string gestureTemplateName = "MyGesture"; // ģ�����ƣ������ڲɼ�ʱָ��
    private string folderPath; // �洢����ģ����ļ���·��

    private void Start()
    {
        gesture_Data_Con = GetComponent<Gesture_Data_Con>();
        folderPath = Application.dataPath + "/Gesture_Tem"; // ����ģ��洢·��
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // ����ļ��в����ڣ������ļ���
        }
    }

    // ��������ģ��
    public void SaveGestureTemplate()
    {
        GestureTemplate gestureTemplate = new GestureTemplate()
        {
            gestureName = gestureTemplateName,
            Finger_Extension = gesture_Data_Con.Finger_Extension, // ������ָ��չ��
            Fingertips_Angle = gesture_Data_Con.Fingertips_Angle, // ������ָ�Ƕ�����
            Fingertips_Distance = gesture_Data_Con.Fingertips_Distance, // ������ָ�����ĵľ�������
            Fingertips_Mutual_Distance = gesture_Data_Con.Fingertips_Mutual_Distance // ������ָ���໥��������
        };

        string json = JsonUtility.ToJson(gestureTemplate);
        string filePath = folderPath + "/" + gestureTemplateName + ".json";
        File.WriteAllText(filePath, json);
        Debug.Log("����ģ���ѱ��棺" + filePath);
    }

    // ��������ģ��
    public GestureTemplate LoadGestureTemplate(string templateName)
    {
        string filePath = folderPath + "/" + templateName + ".json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GestureTemplate loadedTemplate = JsonUtility.FromJson<GestureTemplate>(json);

            Debug.Log("����ģ����سɹ���" + filePath);
            return loadedTemplate;
        }
        else
        {
            Debug.LogError("����ģ���ļ�δ�ҵ���" + filePath);
            return null;
        }
    }

    // �����������
    public void ClearGestureData()
    {
        gesture_Data_Con.Finger_Extension = 0f;
        gesture_Data_Con.Fingertips_Angle = 0f;
        gesture_Data_Con.Fingertips_Distance = 0f;
        gesture_Data_Con.Fingertips_Mutual_Distance = 0f;
    }

    // ����ģ�����ݽṹ
    [System.Serializable]
    public class GestureTemplate
    {
        public string gestureName;
        public float Finger_Extension; // ��ָ��չ��
        public float Fingertips_Angle; // ��ָ�Ƕ�����
        public float Fingertips_Distance; // ��ָ�����ĵľ�������
        public float Fingertips_Mutual_Distance; // ��ָ���໥��������
    }
}
