using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMM_CAR : MonoBehaviour
{
    public float Car_Speed = 1.0f;
    public GameObject Car;
    public Camera Follow_Camera;
    private bool Car_Turn = false;
    public float distance = 5f;
    public float hight = 5f;
    private float totalTurnAngle = 0f; // 记录总共转过的角度
    public float maxTurnAngle = 30f; // 最大转弯角度
    public float Car_Jump_Force = 3f;
    private bool Car_Is_Jumping = false;
    private Rigidbody Car_Rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        Car_Rigidbody = Car.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Car_Turn)
        {
            // 计算转弯半径
            float radius = Car_Speed / (30 * Mathf.Deg2Rad);
            // 计算角速度
            float angularSpeed = Car_Speed / radius;
            // 计算旋转角度
            float angle = angularSpeed * Time.deltaTime;
            // 检查是否超过最大转弯角度
            if (totalTurnAngle + angle * Mathf.Rad2Deg >= maxTurnAngle)
            {
                // 调整最后一帧的旋转角度，使总旋转角度等于最大转弯角度
                angle = (maxTurnAngle - totalTurnAngle) * Mathf.Deg2Rad;
                Car_Turn = false;
                totalTurnAngle = 0f; // 重置总旋转角度
            }
            else
            {
                totalTurnAngle += angle * Mathf.Rad2Deg;
            }

            // 围绕车辆的上方轴旋转
            Car.transform.Rotate(Vector3.up, 1 * angle * Mathf.Rad2Deg, Space.World);
            // 沿着圆弧路径移动
            Car.transform.Translate(Vector3.forward * Car_Speed * Time.deltaTime, Space.Self);
            
        }
        else
        {
            Car.transform.Translate(Vector3.right * Car_Speed * Time.deltaTime);
        }

        Vector3 Follow_Camera_Position = Car.transform.position-Car.transform.right*distance+Car.transform.up*hight;
        Follow_Camera.transform.position = Follow_Camera_Position;
        Follow_Camera.transform.LookAt(Car.transform);
    }
    public void Car_Rotate()
    {
        Car_Turn = true;
        totalTurnAngle = 0f; // 开始转弯时重置总旋转角度
    }
    public void Camera_Near()
    {
        distance = distance - 1;
        hight = hight - 1;
    }
    public void Car_Jump()
    {
        if(!Car_Is_Jumping)
        {
            Car_Is_Jumping = true;
            Car_Rigidbody.AddForce(Vector3.up * Car_Jump_Force);
            StartCoroutine(WaitForLanding());
        }
    }
    IEnumerator WaitForLanding()
    {
        while(Car_Rigidbody.velocity.y>0.01f)
        {
            yield return null;
        }
        Car_Is_Jumping=false;
    }
}
