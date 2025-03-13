using UnityEngine;
using System.IO;
using System.Collections.Generic; // ����List����

public class Gesture_Dynamic_Template : MonoBehaviour
{
    public Gesture_Dynamic_Data_Con gesture_Data_Con;
    public string gestureTemplateName = "MyDynamicGesture"; // ģ������
    private string folderPath; // �洢����ģ����ļ���·��

    private void Start()
    {
        gesture_Data_Con = GetComponent<Gesture_Dynamic_Data_Con>();
        folderPath = Application.dataPath + "/Dynamic_Gesture_Tem/WoQuan"; // ����ģ��洢·��
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
            Palm_Movement_List = gesture_Data_Con.Palm_Movement_List,
            Finger_Extension_List = gesture_Data_Con.Finger_Extension_List,
            Palm_Angle_List = gesture_Data_Con.Palm_Angle_List,
            Movement_Rate_List = gesture_Data_Con.Movement_Rate_List
        };

        string json = JsonUtility.ToJson(gestureTemplate);
        string filePath = folderPath + "/" + gestureTemplateName + ".json";
        File.WriteAllText(filePath, json);
        Debug.Log("Dynamic Gesture Template Saved: " + filePath);
    }

    // ��������ģ��
    public GestureTemplate LoadGestureTemplate(string templateName)
    {
        string filePath = folderPath + "/" + templateName + ".json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GestureTemplate loadedTemplate = JsonUtility.FromJson<GestureTemplate>(json);

            Debug.Log("Dynamic Gesture Template Loaded: " + filePath);
            return loadedTemplate;
        }
        else
        {
            Debug.LogError("Gesture Template Not Found: " + filePath);
            return null;
        }
    }

    // ����ģ�����ݽṹ
    [System.Serializable]
    public class GestureTemplate
    {
        public string gestureName;
        public List<float> Palm_Movement_List; // ����λ��ʱ������
        public List<float> Finger_Extension_List; // ��ָ��չ��ʱ������
        public List<float> Palm_Angle_List; // ���ĽǶ�ʱ������
        public List<float> Movement_Rate_List; // �˶�����仯��ʱ������
    }
}
