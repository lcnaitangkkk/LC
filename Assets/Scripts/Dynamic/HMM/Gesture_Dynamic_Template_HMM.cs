using UnityEngine;
using System.IO;
using System.Collections.Generic; // ����List����

public class Gesture_Dynamic_Template_HMM : MonoBehaviour
{
    public Gesture_Dynamic_Data_Con_HMM gesture_Data_Con;
    public string gestureTemplateName = "MyDynamicGesture"; // ģ������
    private string folderPath; // �洢����ģ����ļ���·��

    private void Start()
    {
        gesture_Data_Con = GetComponent<Gesture_Dynamic_Data_Con_HMM>();
        folderPath = Application.dataPath + "/Dynamic_Gesture_Tem_Test"; // ����ģ��洢·��
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // ����ļ��в����ڣ��򴴽��ļ���
        }
    }

    // ��������ģ��
    public void SaveGestureTemplate()
    {
        GestureTemplate gestureTemplate = new GestureTemplate()
        {
            gestureName = gestureTemplateName,
            Palm_Movement_List = gesture_Data_Con.GetPalmMovementList(),
            Finger_Extension_List = gesture_Data_Con.GetFingerExtensionList(),
            Movement_Rate_List = gesture_Data_Con.GetMovementRateList(),
            Palm_Movement_Direction_List = gesture_Data_Con.GetPalmMovementDirectionList(),
            Palm_Rotation_Quaternion_List = gesture_Data_Con.GetPalmRotationQuaternionList()
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

    // ģ�����ݽṹ
    [System.Serializable]
    public class GestureTemplate
    {
        public string gestureName;
        public List<float> Palm_Movement_List; // ����λ��ʱ������
        public List<float> Finger_Extension_List; // ��ָ��չ��ʱ������
        public List<float> Movement_Rate_List; // �˶�����仯��ʱ������
        public List<Vector3> Palm_Movement_Direction_List; // �����ƶ�����ʱ������
        public List<Quaternion> Palm_Rotation_Quaternion_List; // ������ת��Ԫ��ʱ������
    }
}