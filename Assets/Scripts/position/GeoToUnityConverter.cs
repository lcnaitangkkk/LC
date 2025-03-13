using System;
using UnityEngine;

/// <summary>
/// һ�������࣬���ڽ��������꣨��γ�ȡ��߶ȣ�ת��Ϊ Unity �������ꡣ
/// </summary>
public class GeoToUnityConverter : MonoBehaviour
{
    // WGS84 ������
    private const double a = 6378137.0; // ���򳤰���
    private const double b = 6356752.314245; // ����̰���
    private const double e2 = 1 - (b * b) / (a * a); // ƫ����ƽ��

    // Cesium �� ECEF ԭ�㣨�� CesiumGeoreference ��ȡ��
    [SerializeField]
    private Vector3d ecefOrigin = new Vector3d(-2527640.3376, 5304899.7792, 2471572.8627);

    // Cesium �ṩ�� East-Up-North (EUN) ��ת�Ƕ�
    [SerializeField]
    private Vector3 rotationEUN = new Vector3(89.9802f, -179.9977f, 0f);

    // Unity �������ű���
    [SerializeField]
    private Vector3 scale = new Vector3(-1f, -1f, -1f);

    /// <summary>
    /// ���������꣨��γ�Ⱥ͸߶ȣ�ת��Ϊ Unity �������ꡣ
    /// </summary>
    /// <param name="latitude">γ�ȣ���λ���ȣ���</param>
    /// <param name="longitude">���ȣ���λ���ȣ���</param>
    /// <param name="height">�߶ȣ���λ���ף���</param>
    /// <returns>ת����� Unity �������ꡣ</returns>
    public Vector3 LatLonToUnity(double latitude, double longitude, double height)
    {
        // Step 1: ��γ��ת����
        double latRad = latitude * Mathf.Deg2Rad;
        double lonRad = longitude * Mathf.Deg2Rad;

        // Step 2: ���㷨�����ʰ뾶
        double N = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(latRad), 2));

        // Step 3: ���� ECEF ����
        double x = (N + height) * Math.Cos(latRad) * Math.Cos(lonRad);
        double y = (N + height) * Math.Cos(latRad) * Math.Sin(lonRad);
        double z = (N * (1 - e2) + height) * Math.Sin(latRad);

        Debug.Log($"Computed ECEF Coordinates: X={x}, Y={y}, Z={z}");

        // Step 4: ������� ECEF ԭ���ƫ��
        double offsetX = x - ecefOrigin.x;
        double offsetY = y - ecefOrigin.y;
        double offsetZ = z - ecefOrigin.z;

        Debug.Log($"Offset relative to ECEF Origin: OffsetX={offsetX}, OffsetY={offsetY}, OffsetZ={offsetZ}");

        // ת��Ϊ Unity ���꣨�м�����δ������ת�����ţ�
        Vector3 relativePosition = new Vector3((float)offsetX, (float)offsetY, (float)offsetZ);

        Debug.Log($"Relative Position (before rotation): {relativePosition}");

        // Step 5: Ӧ�� East-Up-North (EUN) ��ת
        relativePosition = ApplyRotation(relativePosition);

        Debug.Log($"Rotated Position: {relativePosition}");

        // Step 6: Ӧ�����ţ�������ϵת��������ϵ��
        Vector3 unityPosition = Vector3.Scale(relativePosition, scale);

        Debug.Log($"Final Unity Position: {unityPosition}");
        return unityPosition;
    }

    /// <summary>
    /// Ӧ�� Cesium �� East-Up-North (EUN) ��ת��
    /// </summary>
    /// <param name="position">����� ECEF ԭ���λ�á�</param>
    /// <returns>��ת��� Unity ���ꡣ</returns>
    private Vector3 ApplyRotation(Vector3 position)
    {

        
        Quaternion rotationQuat = Quaternion.Euler(rotationEUN);
        return rotationQuat * position;
    }
}

/// <summary>
/// ֧��˫���ȵ� 3D �����ṹ�塣
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
