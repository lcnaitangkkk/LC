using System;
using UnityEngine;

/// <summary>
/// 一个工具类，用于将地理坐标（经纬度、高度）转换为 Unity 世界坐标。
/// </summary>
public class GeoToUnityConverter : MonoBehaviour
{
    // WGS84 椭球常量
    private const double a = 6378137.0; // 地球长半轴
    private const double b = 6356752.314245; // 地球短半轴
    private const double e2 = 1 - (b * b) / (a * a); // 偏心率平方

    // Cesium 的 ECEF 原点（从 CesiumGeoreference 获取）
    [SerializeField]
    private Vector3d ecefOrigin = new Vector3d(-2527640.3376, 5304899.7792, 2471572.8627);

    // Cesium 提供的 East-Up-North (EUN) 旋转角度
    [SerializeField]
    private Vector3 rotationEUN = new Vector3(89.9802f, -179.9977f, 0f);

    // Unity 坐标缩放比例
    [SerializeField]
    private Vector3 scale = new Vector3(-1f, -1f, -1f);

    /// <summary>
    /// 将地理坐标（经纬度和高度）转换为 Unity 世界坐标。
    /// </summary>
    /// <param name="latitude">纬度（单位：度）。</param>
    /// <param name="longitude">经度（单位：度）。</param>
    /// <param name="height">高度（单位：米）。</param>
    /// <returns>转换后的 Unity 世界坐标。</returns>
    public Vector3 LatLonToUnity(double latitude, double longitude, double height)
    {
        // Step 1: 经纬度转弧度
        double latRad = latitude * Mathf.Deg2Rad;
        double lonRad = longitude * Mathf.Deg2Rad;

        // Step 2: 计算法线曲率半径
        double N = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(latRad), 2));

        // Step 3: 计算 ECEF 坐标
        double x = (N + height) * Math.Cos(latRad) * Math.Cos(lonRad);
        double y = (N + height) * Math.Cos(latRad) * Math.Sin(lonRad);
        double z = (N * (1 - e2) + height) * Math.Sin(latRad);

        Debug.Log($"Computed ECEF Coordinates: X={x}, Y={y}, Z={z}");

        // Step 4: 计算相对 ECEF 原点的偏移
        double offsetX = x - ecefOrigin.x;
        double offsetY = y - ecefOrigin.y;
        double offsetZ = z - ecefOrigin.z;

        Debug.Log($"Offset relative to ECEF Origin: OffsetX={offsetX}, OffsetY={offsetY}, OffsetZ={offsetZ}");

        // 转换为 Unity 坐标（中间结果，未经过旋转和缩放）
        Vector3 relativePosition = new Vector3((float)offsetX, (float)offsetY, (float)offsetZ);

        Debug.Log($"Relative Position (before rotation): {relativePosition}");

        // Step 5: 应用 East-Up-North (EUN) 旋转
        relativePosition = ApplyRotation(relativePosition);

        Debug.Log($"Rotated Position: {relativePosition}");

        // Step 6: 应用缩放（从右手系转换到左手系）
        Vector3 unityPosition = Vector3.Scale(relativePosition, scale);

        Debug.Log($"Final Unity Position: {unityPosition}");
        return unityPosition;
    }

    /// <summary>
    /// 应用 Cesium 的 East-Up-North (EUN) 旋转。
    /// </summary>
    /// <param name="position">相对于 ECEF 原点的位置。</param>
    /// <returns>旋转后的 Unity 坐标。</returns>
    private Vector3 ApplyRotation(Vector3 position)
    {

        
        Quaternion rotationQuat = Quaternion.Euler(rotationEUN);
        return rotationQuat * position;
    }
}

/// <summary>
/// 支持双精度的 3D 向量结构体。
/// </summary>
[Serializable]
public struct Vector3d
{
    public double x, y, z;

    public Vector3d(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3d operator -(Vector3d a, Vector3d b)
    {
        return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
    }
}
