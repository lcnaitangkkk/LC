using UnityEngine;
using System.IO;
using System.Collections.Generic; // 引入List类型

public class Gesture_Dynamic_Template : MonoBehaviour
{
    public Gesture_Dynamic_Data_Con gesture_Data_Con;
    public string gestureTemplateName = "MyDynamicGesture"; // 模板名称
    private string folderPath; // 存储手势模板的文件夹路径

    private void Start()
    {
        gesture_Data_Con = GetComponent<Gesture_Dynamic_Data_Con>();
        folderPath = Application.dataPath + "/Dynamic_Gesture_Tem/WoQuan"; // 手势模板存储路径
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // 如果文件夹不存在，创建文件夹
        }
    }

    // 保存手势模板
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

    // 加载手势模板
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

    // 手势模板数据结构
    [System.Serializable]
    public class GestureTemplate
    {
        public string gestureName;
        public List<float> Palm_Movement_List; // 掌心位移时间序列
        public List<float> Finger_Extension_List; // 手指伸展度时间序列
        public List<float> Palm_Angle_List; // 掌心角度时间序列
        public List<float> Movement_Rate_List; // 运动方向变化率时间序列
    }
}
