//using Mirage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movetest : MonoBehaviour
{

    public float sbeve=1;

    void Update()
    {

        //if (IsLocalPlayer)
        //{
        //    Move();

        //}
        
    }

    void Move()
    {
        Vector2 moveDir=Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            moveDir.y = 1;
        else if (Input.GetKey(KeyCode.DownArrow))
            moveDir.y = -1;

        if (Input.GetKey(KeyCode.RightArrow))
            moveDir.x = 1;
        else if (Input.GetKey(KeyCode.LeftArrow))
            moveDir.x = -1;

        moveDir *= sbeve;

        transform.position = new Vector3(transform.position.x + moveDir.x, transform.position.y + moveDir.y, transform.position.z);
    }
}
