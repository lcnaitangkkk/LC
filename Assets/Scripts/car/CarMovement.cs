using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float radius = 3f;  // ���ι���İ뾶
    public float speed = 2f;    // С�����ٶ�
    private float angle = 0f;   // С����ǰ�ĽǶ�

    void Update()
    {
        // ����С���� X �� Z ���꣬Y ���걣�ֲ���
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // ����С����λ��
        transform.position = new Vector3(x, transform.position.y, z);

        // ��С�����Ź���ƶ�
        angle += speed * Time.deltaTime;  // ���½Ƕ�

        // ȷ���Ƕ��� 0 �� 360 ��֮��
        if (angle >= 360f)
        {
            angle -= 360f;
        }
    }
}
