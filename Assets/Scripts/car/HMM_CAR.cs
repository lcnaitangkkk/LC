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
    private float totalTurnAngle = 0f; // ��¼�ܹ�ת���ĽǶ�
    public float maxTurnAngle = 30f; // ���ת��Ƕ�
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
            // ����ת��뾶
            float radius = Car_Speed / (30 * Mathf.Deg2Rad);
            // ������ٶ�
            float angularSpeed = Car_Speed / radius;
            // ������ת�Ƕ�
            float angle = angularSpeed * Time.deltaTime;
            // ����Ƿ񳬹����ת��Ƕ�
            if (totalTurnAngle + angle * Mathf.Rad2Deg >= maxTurnAngle)
            {
                // �������һ֡����ת�Ƕȣ�ʹ����ת�Ƕȵ������ת��Ƕ�
                angle = (maxTurnAngle - totalTurnAngle) * Mathf.Deg2Rad;
                Car_Turn = false;
                totalTurnAngle = 0f; // ��������ת�Ƕ�
            }
            else
            {
                totalTurnAngle += angle * Mathf.Rad2Deg;
            }

            // Χ�Ƴ������Ϸ�����ת
            Car.transform.Rotate(Vector3.up, 1 * angle * Mathf.Rad2Deg, Space.World);
            // ����Բ��·���ƶ�
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
        totalTurnAngle = 0f; // ��ʼת��ʱ��������ת�Ƕ�
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
