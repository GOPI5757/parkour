using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{

    public float runSpeed, SlopeSpeed;
    public float dashForce;
    float moveSpeed;

    float MouseX, MouseY;

    float horiz, verti;
    public float JumpForce;
    public GameObject CamParent;

    float xRotation = 0f;
    float yRotation = 0f;

    float SlideTimeRun = 0f;

    float SlideTimer = 1f;

    public bool IsSliding = false;

    bool IsSlope = false;

    bool IsGrounded = false;

    public Quaternion endTransformLeft, endTransformRight;

    bool canGoLeft = true, canGoRight = true, canAffectMouseY = true, canGoForward = true; 

    Rigidbody rb;

    public static Player Instance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if(Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        moveSpeed = runSpeed;
    }

    void Update()
    {
        Cursor.visible = false;
        horiz = Input.GetAxisRaw("Horizontal");
        verti = Input.GetAxisRaw("Vertical");

        if (!canGoLeft)
        {
            if (horiz < 0)
            {
                horiz = 0;
            }
        }

        if (!canGoRight)
        {
            if(horiz > 0)
            {
                horiz = 0;
            }
        }

        if(!canGoForward)
        {
            if(verti > 0)
            {
                verti = 0f;
            }
        }

        if(IsSlope)
        {
            transform.Translate(0f, 0f, SlopeSpeed * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.LeftControl) && !IsSliding && (canGoLeft && canGoRight) && IsGrounded)
        {
            if(verti > 0)
            {
                rb.AddForce(transform.forward * 10f, ForceMode.Impulse);
                CamParent.GetComponent<Animator>().SetBool("CanSlide", true);
                IsSliding = true;
            }
        }

        if(IsSliding)
        {
            SlideTimeRun += Time.deltaTime;
            if(SlideTimeRun >= SlideTimer)
            {
                SlideTimeRun = 0f;
                IsSliding = false;
                CamParent.GetComponent<Animator>().SetBool("CanSlide", false);
            }
        }

        if (!canGoLeft)
        {
            CamParent.GetComponent<Animator>().SetBool("CanTiltRight", true);
        } else
        {
            CamParent.GetComponent<Animator>().SetBool("CanTiltRight", false);
        }

        if (!canGoRight)
        {
            CamParent.GetComponent<Animator>().SetBool("CanTiltLeft", true);
        }
        else
        {
            CamParent.GetComponent<Animator>().SetBool("CanTiltLeft", false);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        DoMovement();
        MouseControl();
    }

    void MouseControl()
    {
        MouseX = Input.GetAxisRaw("Mouse X");
        MouseY = Input.GetAxisRaw("Mouse Y");

        xRotation -= MouseY;
        yRotation += MouseX;

        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        transform.localRotation = Quaternion.Euler(transform.rotation.x, yRotation, transform.rotation.z);
        if(canAffectMouseY)
        {
            Camera.main.transform.localRotation = Quaternion.Euler(xRotation, transform.rotation.y, Camera.main.transform.localRotation.z);
        }
    }

    void DoMovement()
    {
        rb.AddForce(horiz * moveSpeed * Time.deltaTime, 0f, verti * moveSpeed * Time.deltaTime);
    }

    void Jump()
    {
        if(IsSliding || !IsGrounded) return;
        rb.AddForce(0f, JumpForce, 0f);
        IsGrounded = false; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            IsGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            IsGrounded = false;
        }
    }

    public void OnHitSlope()
    {
        IsSlope = true;
        CamParent.GetComponent<Animator>().SetBool("IsSlope", true);
    }
}
