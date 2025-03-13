using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float radius = 3f;  // 环形轨道的半径
    public float speed = 2f;    // 小车的速度
    private float angle = 0f;   // 小车当前的角度

    void Update()
    {
        // 计算小车的 X 和 Z 坐标，Y 坐标保持不变
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // 设置小车的位置
        transform.position = new Vector3(x, transform.position.y, z);

        // 让小车沿着轨道移动
        angle += speed * Time.deltaTime;  // 更新角度

        // 确保角度在 0 到 360 度之间
        if (angle >= 360f)
        {
            angle -= 360f;
        }
    }
}
