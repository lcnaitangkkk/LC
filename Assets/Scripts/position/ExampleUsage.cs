using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    void Start()
    {
        GeoToUnityConverter converter = FindObjectOfType<GeoToUnityConverter>();

        // ʾ����γ�Ⱥ͸߶�
        double latitude = 22.949483;  // γ��
        double longitude = 115.47652857;  // ����
        double height = 10.7771;  // �߶�

        // ת��Ϊ Unity ��������
      Vector3 unityPosition = converter.LatLonToUnity(latitude, longitude, height);

      Debug.Log($"Unity Position: {unityPosition}");
    }



}
