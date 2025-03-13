using UnityEngine;

public class CoordinateConverter : MonoBehaviour
{
    // ����ĳ�����ͱ���
    const double a = 6378137.0; // WGS84����ĳ����ᣬ��λ����
    const double f = 1.0 / 298.257223563; // WGS84����ı���
    const double e2 = 2 * f - f * f; // WGS84�ĵ�һƫ���ʵ�ƽ��

    // ��γ��ת��ΪUnity����ϵ
    public Vector3 ConvertToUnityCoordinates(double latitude, double longitude, double height)
    {
        // ����γ�ȴӶ�ת��Ϊ����
        double phi = latitude * Mathf.Deg2Rad;  // γ��
        double lambda = longitude * Mathf.Deg2Rad;  // ����

        // ������������ʰ뾶 N
        double N = a / Mathf.Sqrt((float)(1 - e2 * Mathf.Sin((float)phi) * Mathf.Sin((float)phi)));

        // ����ѿ�������ϵ�е�X, Y, Z
        double X = (N + height) * Mathf.Cos((float)phi) * Mathf.Cos((float)lambda);
        double Y = (N + height) * Mathf.Cos((float)phi) * Mathf.Sin((float)lambda);
        double Z = ((1 - e2) * N + height) * Mathf.Sin((float)phi);

        // �����ת��ΪUnity��Vector3����ϵ������
        return new Vector3((float)X, (float)Z, (float)Y);  // Unity�е�X��Ϊ������Y��Ϊ��ֱ��Z��Ϊ�ϱ�
    }

    void Start()
    {
        // ʾ�����������ľ�γ��ת��ΪUnity����
        Vector3 unityCoordinates = ConvertToUnityCoordinates(22.94962121, 115.47690259, 5.2363);
        Debug.Log("Unity����: " + unityCoordinates);
    }
}
