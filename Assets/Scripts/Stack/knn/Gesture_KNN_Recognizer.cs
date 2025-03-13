using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class Gesture_KNN_Recognizer : MonoBehaviour
{
    public int k = 3; // K值
    public List<GestureData> dataset; // 存储所有手势模板数据

    // 手势数据类，保存手势的特征
    [System.Serializable]
    public class GestureData
    {
        public string gestureName;
        public float Finger_Extension; // 手指伸展度
        public float Fingertips_Angle; // 手指角度特征
        public float Fingertips_Distance; // 手指与掌心的距离特征
        public float Fingertips_Mutual_Distance; // 手指尖相互距离特征
    }

    // 加载手势模板数据
    public void LoadGestureDataset(string folderPath)
    {
        dataset = new List<GestureData>();

        // 遍历文件夹中的所有JSON文件
        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");
        foreach (var file in jsonFiles)
        {
            string json = File.ReadAllText(file);
            GestureData gesture = JsonUtility.FromJson<GestureData>(json);

            // 处理 NaN 值，将其替换为默认值（如0）
            if (float.IsNaN(gesture.Fingertips_Distance))
                gesture.Fingertips_Distance = 0f;
            if (float.IsNaN(gesture.Fingertips_Mutual_Distance))
                gesture.Fingertips_Mutual_Distance = 0f;

            // 打印加载的手势信息
            Debug.Log($"加载的手势文件：{file}, 手势名称: {gesture.gestureName}");

            dataset.Add(gesture);

            // 打印出加载的手势名称，确认是否加载成功
            Debug.Log($"加载手势：{gesture.gestureName}, Finger Extension: {gesture.Finger_Extension}, Fingertips Angle: {gesture.Fingertips_Angle}, Fingertips Distance: {gesture.Fingertips_Distance}, Fingertips Mutual Distance: {gesture.Fingertips_Mutual_Distance}");
        }

        Debug.Log("手势模板数据集已加载");
    }

    // KNN识别，返回最匹配的手势名称
    public string RecognizeGesture(float fingerExtension, float fingertipsAngle, float fingertipsDistance, float fingertipsMutualDistance)
    {
        // 计算所有手势与当前手势的距离
        List<GestureData> neighbors = GetKNearestNeighbors(fingerExtension, fingertipsAngle, fingertipsDistance, fingertipsMutualDistance);

        // 统计 K 个邻居中出现最多的手势
        Dictionary<string, int> gestureCounts = new Dictionary<string, int>();
        foreach (var neighbor in neighbors)
        {
            if (!gestureCounts.ContainsKey(neighbor.gestureName))
                gestureCounts[neighbor.gestureName] = 0;

            gestureCounts[neighbor.gestureName]++;
        }

        // 打印出识别的邻居手势名称
        foreach (var neighbor in neighbors)
        {
            Debug.Log($"邻居手势：{neighbor.gestureName}, Finger Extension: {neighbor.Finger_Extension}, Fingertips Angle: {neighbor.Fingertips_Angle}, Fingertips Distance: {neighbor.Fingertips_Distance}, Fingertips Mutual Distance: {neighbor.Fingertips_Mutual_Distance}");
        }

        // 找到出现次数最多的手势
        string recognizedGesture = "";
        int maxCount = 0;
        foreach (var kvp in gestureCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                recognizedGesture = kvp.Key;
            }
        }

        // 打印出最终识别的手势
        Debug.Log($"识别的手势：{recognizedGesture}");

        return recognizedGesture;
    }

    // 计算与当前手势的距离，返回 K 个最近邻（使用欧式距离）
    private List<GestureData> GetKNearestNeighbors(float fingerExtension, float fingertipsAngle, float fingertipsDistance, float fingertipsMutualDistance)
    {
        // 特征权重：根据实际情况调整这些权重
        float extensionWeight = 1000f;
        float angleWeight = 1f;
        float distanceWeight = 1f;
        float mutualDistanceWeight = 1f;

        // 计算距离
        List<(float distance, GestureData gesture)> distances = new List<(float, GestureData)>();

        foreach (var gesture in dataset)
        {
            // 计算各个特征之间的欧式距离（平方差求和）
            float fingerDistance = Mathf.Pow(fingerExtension - gesture.Finger_Extension, 2) * extensionWeight;
            float angleDistance = Mathf.Pow(fingertipsAngle - gesture.Fingertips_Angle, 2) * angleWeight;
            float distanceToPalm = Mathf.Pow(fingertipsDistance - gesture.Fingertips_Distance, 2) * distanceWeight;
            float mutualDistance = Mathf.Pow(fingertipsMutualDistance - gesture.Fingertips_Mutual_Distance, 2) * mutualDistanceWeight;

            // 总距离是所有特征距离的加权和
            float totalDistance = Mathf.Sqrt(fingerDistance + angleDistance + distanceToPalm + mutualDistance);

            distances.Add((totalDistance, gesture));
        }

        // 按距离排序，取最小的 K 个
        distances.Sort((a, b) => a.distance.CompareTo(b.distance));

        // 打印出排序后的结果
        Debug.Log("排序后的距离和手势数据：");
        foreach (var (distance, gesture) in distances)
        {
            Debug.Log($"手势：{gesture.gestureName}, 总距离：{distance}, 伸展度：{gesture.Finger_Extension}, 角度：{gesture.Fingertips_Angle}, 距离：{gesture.Fingertips_Distance}, 相互距离：{gesture.Fingertips_Mutual_Distance}");
        }

        List<GestureData> nearestNeighbors = new List<GestureData>();
        for (int i = 0; i < k; i++)
        {
            nearestNeighbors.Add(distances[i].gesture);
        }

        return nearestNeighbors;
    }
}