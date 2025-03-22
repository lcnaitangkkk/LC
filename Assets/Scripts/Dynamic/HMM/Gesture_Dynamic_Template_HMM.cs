using UnityEngine;
using System.IO;
using System.Collections.Generic; // 引入List类型

public class Gesture_Dynamic_Template_HMM : MonoBehaviour
{
    public Gesture_Dynamic_Data_Con_HMM gesture_Data_Con;
    public string gestureTemplateName = "MyDynamicGesture"; // 模板名称
    private string folderPath; // 存储手势模板的文件夹路径

    private void Start()
    {
        gesture_Data_Con = GetComponent<Gesture_Dynamic_Data_Con_HMM>();
        folderPath = Application.dataPath + "/Dynamic_Gesture_Tem_Test"; // 设置模板存储路径
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // 如果文件夹不存在，则创建文件夹
        }
    }

    // 保存手势模板
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

    // 模板数据结构
    [System.Serializable]
    public class GestureTemplate
    {
        public string gestureName;
        public List<float> Palm_Movement_List; // 掌心位移时间序列
        public List<float> Finger_Extension_List; // 手指伸展度时间序列
        public List<float> Movement_Rate_List; // 运动方向变化率时间序列
        public List<Vector3> Palm_Movement_Direction_List; // 手掌移动方向时间序列
        public List<Quaternion> Palm_Rotation_Quaternion_List; // 手掌旋转四元数时间序列
    }
}