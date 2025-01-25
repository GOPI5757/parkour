using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{

    [SerializeField]
    float InitialSpeed, speed, sprintSpeed, SlopeSpeed;

    float MouseX, MouseY;

    public float x, z;

    [SerializeField]
    float JumpForce;

    [SerializeField]
    public GameObject CamParent;

    float xRotation = 0f;
    float yRotation = 0f;

    [SerializeField]
    float SlideTimeRun = 0f;

    [SerializeField]
    float SlideTimer = 1f;

    [SerializeField]
    public bool IsSliding = false;

    [SerializeField]
    public bool IsSlope = false;

    [SerializeField]
    public bool IsEnteringSlide = false;

    [SerializeField]
    public bool IsGrounded = false;

    [SerializeField]
    public bool IsSprinting = false;

    public Quaternion endTransformLeft, endTransformRight;

    [SerializeField]
    public bool canGoLeft = true, canGoRight = true, canAffectMouseY = true, canGoForward = true; 

    public Rigidbody rb;

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
        speed = InitialSpeed;
    }

    void Update()
    {
        Cursor.visible = false;
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");

        if (!canGoLeft)
        {
            if (x < 0)
            {
                x = 0;
            }
        }

        if (!canGoRight)
        {
            if(x > 0)
            {
                x = 0;
            }
        }

        if(!canGoForward)
        {
            if(z > 0)
            {
                z = 0f;
            }
        }

        if(IsSlope)
        {
            transform.Translate(0f, 0f, SlopeSpeed * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.LeftControl) && !IsSliding && (canGoLeft && canGoRight) && IsGrounded)
        {
            if(z > 0)
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

        if(Input.GetKey(KeyCode.LeftShift))
        {
            IsSprinting = true;
            speed = sprintSpeed;
        } else
        {
            IsSprinting = false;
            speed = InitialSpeed;
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

        transform.Translate(x * speed * Time.deltaTime, 0f, z * speed * Time.deltaTime);
        if(Input.GetKeyDown(KeyCode.Space) && !IsSliding)
        {
            rb.AddForce(0f, JumpForce, 0f);
            IsGrounded = false;
        }

        /*if(IsEnteringSlide)
        {
            CamParent.transform.rotation = Quaternion.Lerp(CamParent.transform.rotation, Quaternion.EulerAngles(0f, 0f, 0f), Time.deltaTime * 10f);
        }*/

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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            IsGrounded = true;
        }

        
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.tag == "slopeSlide")
        {
            CamParent.GetComponent<Animator>().SetBool("IsSlope", true);
        }*/
    }
}
