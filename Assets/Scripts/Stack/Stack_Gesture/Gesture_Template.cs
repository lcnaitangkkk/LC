using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Gesture_Template : MonoBehaviour
{
    public Gesture_Data_Con gesture_Data_Con;
    public string gestureTemplateName = "MyGesture"; // 模板名称，可以在采集时指定
    private string folderPath; // 存储手势模板的文件夹路径

    private void Start()
    {
        gesture_Data_Con = GetComponent<Gesture_Data_Con>();
        folderPath = Application.dataPath + "/Gesture_Tem"; // 手势模板存储路径
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
            Finger_Extension = gesture_Data_Con.Finger_Extension, // 保存手指伸展度
            Fingertips_Angle = gesture_Data_Con.Fingertips_Angle, // 保存手指角度特征
            Fingertips_Distance = gesture_Data_Con.Fingertips_Distance, // 保存手指与掌心的距离特征
            Fingertips_Mutual_Distance = gesture_Data_Con.Fingertips_Mutual_Distance // 保存手指尖相互距离特征
        };

        string json = JsonUtility.ToJson(gestureTemplate);
        string filePath = folderPath + "/" + gestureTemplateName + ".json";
        File.WriteAllText(filePath, json);
        Debug.Log("手势模板已保存：" + filePath);
    }

    // 加载手势模板
    public GestureTemplate LoadGestureTemplate(string templateName)
    {
        string filePath = folderPath + "/" + templateName + ".json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GestureTemplate loadedTemplate = JsonUtility.FromJson<GestureTemplate>(json);

            Debug.Log("手势模板加载成功：" + filePath);
            return loadedTemplate;
        }
        else
        {
            Debug.LogError("手势模板文件未找到：" + filePath);
            return null;
        }
    }

    // 清除手势数据
    public void ClearGestureData()
    {
        gesture_Data_Con.Finger_Extension = 0f;
        gesture_Data_Con.Fingertips_Angle = 0f;
        gesture_Data_Con.Fingertips_Distance = 0f;
        gesture_Data_Con.Fingertips_Mutual_Distance = 0f;
    }

    // 手势模板数据结构
    [System.Serializable]
    public class GestureTemplate
    {
        public string gestureName;
        public float Finger_Extension; // 手指伸展度
        public float Fingertips_Angle; // 手指角度特征
        public float Fingertips_Distance; // 手指与掌心的距离特征
        public float Fingertips_Mutual_Distance; // 手指尖相互距离特征
    }
}
