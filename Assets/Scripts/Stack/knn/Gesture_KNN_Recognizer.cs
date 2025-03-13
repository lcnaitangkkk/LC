using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class Gesture_KNN_Recognizer : MonoBehaviour
{
    public int k = 3; // Kֵ
    public List<GestureData> dataset; // �洢��������ģ������

    // ���������࣬�������Ƶ�����
    [System.Serializable]
    public class GestureData
    {
        public string gestureName;
        public float Finger_Extension; // ��ָ��չ��
        public float Fingertips_Angle; // ��ָ�Ƕ�����
        public float Fingertips_Distance; // ��ָ�����ĵľ�������
        public float Fingertips_Mutual_Distance; // ��ָ���໥��������
    }

    // ��������ģ������
    public void LoadGestureDataset(string folderPath)
    {
        dataset = new List<GestureData>();

        // �����ļ����е�����JSON�ļ�
        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");
        foreach (var file in jsonFiles)
        {
            string json = File.ReadAllText(file);
            GestureData gesture = JsonUtility.FromJson<GestureData>(json);

            // ���� NaN ֵ�������滻ΪĬ��ֵ����0��
            if (float.IsNaN(gesture.Fingertips_Distance))
                gesture.Fingertips_Distance = 0f;
            if (float.IsNaN(gesture.Fingertips_Mutual_Distance))
                gesture.Fingertips_Mutual_Distance = 0f;

            // ��ӡ���ص�������Ϣ
            Debug.Log($"���ص������ļ���{file}, ��������: {gesture.gestureName}");

            dataset.Add(gesture);

            // ��ӡ�����ص��������ƣ�ȷ���Ƿ���سɹ�
            Debug.Log($"�������ƣ�{gesture.gestureName}, Finger Extension: {gesture.Finger_Extension}, Fingertips Angle: {gesture.Fingertips_Angle}, Fingertips Distance: {gesture.Fingertips_Distance}, Fingertips Mutual Distance: {gesture.Fingertips_Mutual_Distance}");
        }

        Debug.Log("����ģ�����ݼ��Ѽ���");
    }

    // KNNʶ�𣬷�����ƥ�����������
    public string RecognizeGesture(float fingerExtension, float fingertipsAngle, float fingertipsDistance, float fingertipsMutualDistance)
    {
        // �������������뵱ǰ���Ƶľ���
        List<GestureData> neighbors = GetKNearestNeighbors(fingerExtension, fingertipsAngle, fingertipsDistance, fingertipsMutualDistance);

        // ͳ�� K ���ھ��г�����������
        Dictionary<string, int> gestureCounts = new Dictionary<string, int>();
        foreach (var neighbor in neighbors)
        {
            if (!gestureCounts.ContainsKey(neighbor.gestureName))
                gestureCounts[neighbor.gestureName] = 0;

            gestureCounts[neighbor.gestureName]++;
        }

        // ��ӡ��ʶ����ھ���������
        foreach (var neighbor in neighbors)
        {
            Debug.Log($"�ھ����ƣ�{neighbor.gestureName}, Finger Extension: {neighbor.Finger_Extension}, Fingertips Angle: {neighbor.Fingertips_Angle}, Fingertips Distance: {neighbor.Fingertips_Distance}, Fingertips Mutual Distance: {neighbor.Fingertips_Mutual_Distance}");
        }

        // �ҵ����ִ�����������
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

        // ��ӡ������ʶ�������
        Debug.Log($"ʶ������ƣ�{recognizedGesture}");

        return recognizedGesture;
    }

    // �����뵱ǰ���Ƶľ��룬���� K ������ڣ�ʹ��ŷʽ���룩
    private List<GestureData> GetKNearestNeighbors(float fingerExtension, float fingertipsAngle, float fingertipsDistance, float fingertipsMutualDistance)
    {
        // ����Ȩ�أ�����ʵ�����������ЩȨ��
        float extensionWeight = 1000f;
        float angleWeight = 1f;
        float distanceWeight = 1f;
        float mutualDistanceWeight = 1f;

        // �������
        List<(float distance, GestureData gesture)> distances = new List<(float, GestureData)>();

        foreach (var gesture in dataset)
        {
            // �����������֮���ŷʽ���루ƽ������ͣ�
            float fingerDistance = Mathf.Pow(fingerExtension - gesture.Finger_Extension, 2) * extensionWeight;
            float angleDistance = Mathf.Pow(fingertipsAngle - gesture.Fingertips_Angle, 2) * angleWeight;
            float distanceToPalm = Mathf.Pow(fingertipsDistance - gesture.Fingertips_Distance, 2) * distanceWeight;
            float mutualDistance = Mathf.Pow(fingertipsMutualDistance - gesture.Fingertips_Mutual_Distance, 2) * mutualDistanceWeight;

            // �ܾ�����������������ļ�Ȩ��
            float totalDistance = Mathf.Sqrt(fingerDistance + angleDistance + distanceToPalm + mutualDistance);

            distances.Add((totalDistance, gesture));
        }

        // ����������ȡ��С�� K ��
        distances.Sort((a, b) => a.distance.CompareTo(b.distance));

        // ��ӡ�������Ľ��
        Debug.Log("�����ľ�����������ݣ�");
        foreach (var (distance, gesture) in distances)
        {
            Debug.Log($"���ƣ�{gesture.gestureName}, �ܾ��룺{distance}, ��չ�ȣ�{gesture.Finger_Extension}, �Ƕȣ�{gesture.Fingertips_Angle}, ���룺{gesture.Fingertips_Distance}, �໥���룺{gesture.Fingertips_Mutual_Distance}");
        }

        List<GestureData> nearestNeighbors = new List<GestureData>();
        for (int i = 0; i < k; i++)
        {
            nearestNeighbors.Add(distances[i].gesture);
        }

        return nearestNeighbors;
    }
}