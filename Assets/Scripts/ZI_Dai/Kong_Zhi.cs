using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kong_Zhi : MonoBehaviour
{
    public GameObject cube;
    public GameObject cylinder;
    public GameObject sphere;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void On_P_D()
    {
        cube.GetComponent<Renderer>().material.color = Color.yellow;
    }
    public void O_W_P_D()
    {
        cylinder.GetComponent<Renderer>().material.color = Color.green;
    }
    public void O_P_L()
    {
        cube.GetComponent<Renderer>().material.color = Color.white;
    }
}
