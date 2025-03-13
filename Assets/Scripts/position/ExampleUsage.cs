using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    void Start()
    {
        GeoToUnityConverter converter = FindObjectOfType<GeoToUnityConverter>();

        // 示例经纬度和高度
        double latitude = 22.949483;  // 纬度
        double longitude = 115.47652857;  // 经度
        double height = 10.7771;  // 高度

        // 转换为 Unity 世界坐标
      Vector3 unityPosition = converter.LatLonToUnity(latitude, longitude, height);

      Debug.Log($"Unity Position: {unityPosition}");
    }



}
