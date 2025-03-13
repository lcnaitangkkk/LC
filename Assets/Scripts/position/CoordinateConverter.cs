using UnityEngine;

public class CoordinateConverter : MonoBehaviour
{
    // 地球的长半轴和扁率
    const double a = 6378137.0; // WGS84椭球的长半轴，单位：米
    const double f = 1.0 / 298.257223563; // WGS84椭球的扁率
    const double e2 = 2 * f - f * f; // WGS84的第一偏心率的平方

    // 经纬度转换为Unity坐标系
    public Vector3 ConvertToUnityCoordinates(double latitude, double longitude, double height)
    {
        // 将经纬度从度转换为弧度
        double phi = latitude * Mathf.Deg2Rad;  // 纬度
        double lambda = longitude * Mathf.Deg2Rad;  // 经度

        // 计算椭球的曲率半径 N
        double N = a / Mathf.Sqrt((float)(1 - e2 * Mathf.Sin((float)phi) * Mathf.Sin((float)phi)));

        // 计算笛卡尔坐标系中的X, Y, Z
        double X = (N + height) * Mathf.Cos((float)phi) * Mathf.Cos((float)lambda);
        double Y = (N + height) * Mathf.Cos((float)phi) * Mathf.Sin((float)lambda);
        double Z = ((1 - e2) * N + height) * Mathf.Sin((float)phi);

        // 将结果转换为Unity的Vector3坐标系并返回
        return new Vector3((float)X, (float)Z, (float)Y);  // Unity中的X轴为东西，Y轴为垂直，Z轴为南北
    }

    void Start()
    {
        // 示例：将北京的经纬度转换为Unity坐标
        Vector3 unityCoordinates = ConvertToUnityCoordinates(22.94962121, 115.47690259, 5.2363);
        Debug.Log("Unity坐标: " + unityCoordinates);
    }
}
