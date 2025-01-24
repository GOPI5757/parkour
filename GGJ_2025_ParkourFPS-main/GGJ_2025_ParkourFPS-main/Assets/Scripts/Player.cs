using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField]
    float speed;

    float MouseX, MouseY;

    float x, z;

    [SerializeField]
    float JumpForce;

    float xRotation = 0f;
    float yRotation = 0f;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        Cursor.visible = false;
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");

        transform.Translate(x * speed * Time.deltaTime, 0f, z * speed * Time.deltaTime);
        if(Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(0f, JumpForce, 0f);
        }

        MouseControl();
        //print(gameObject.transform.GetChild(1).tag);
    }

    void MouseControl()
    {
        MouseX = Input.GetAxisRaw("Mouse X");
        MouseY = Input.GetAxisRaw("Mouse Y");
        if(MouseX > 0f ||  MouseY > 0f)
        {
        }
        xRotation -= MouseY;
        yRotation += MouseX;

        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, transform.rotation.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "LeftWall")
    }
}
