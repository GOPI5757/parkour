using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class ParkourController : MonoBehaviour
{

    [Header("Camera")]
    public float mouseSensitivity;
    public GameObject attachedCam;

    [Header("Movement")]
    public TMP_Text speedText;
    public float runForce;
    public float maxSpeed;
    public float groundDrag;
    public float airDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;


    [Header("Jumping")]
    public float jumpForce;
    public float wallJumpMultiplier;
    public float airMultiplier;


    [Header("Wallrunning")]
    public LayerMask wallLayer;
    public float wallRunForce, maxWallRunTime, maxWallRunSpeed;
    public float maxWallRunTilt, tiltSpeed;

    float wallRunCameraTilt;

    bool isWallLeft, isWallRight;
    bool isWallRunning;

    bool isGrounded;
    bool isSprinting;

    Rigidbody rb;

    float horiz, verti;
    float mouseX, mouseY;
    float moveSpeed;
    float yRot, xRot;

    void Start()
    {
        moveSpeed = runForce;
        rb = GetComponent<Rigidbody>();
        Application.targetFrameRate = 60;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {   
        CheckForInput();
        CheckWallInput();
        CheckForWall();
        GroundCheck();
        ChooseMoveSpeed();
        PlayerLook();
        UpdatePlayerDrag();

        Vector3 movementVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        speedText.text = $"Speed: {movementVel.magnitude}";
    }

    void FixedUpdate()
    {
        DoMovement();
        SpeedControl();
    }

    void CheckForInput()
    {
        horiz = Input.GetAxisRaw("Horizontal");
        verti = Input.GetAxisRaw("Vertical");

        mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;
        mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
        }else
        {
            isSprinting = false;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    void CheckWallInput()
    {
        if(horiz == 1 && isWallRight)
        {
            StartWallRun();
        }
        if(horiz == -1 && isWallLeft)
        {
            StartWallRun();
        }
    }

    void PlayerLook()
    {
        yRot += mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        attachedCam.transform.rotation = Quaternion.Euler(xRot, yRot, wallRunCameraTilt);
        transform.rotation = Quaternion.Euler(0f, yRot, 0f);


        //Tilt for Wallrun
        if(isWallRunning && isWallRight && Mathf.Abs(wallRunCameraTilt) < maxWallRunTilt)
        {
            wallRunCameraTilt += tiltSpeed * Time.deltaTime;
        }
        if(isWallRunning && isWallLeft && Mathf.Abs(wallRunCameraTilt) < maxWallRunTilt)
        {
            wallRunCameraTilt -= tiltSpeed * Time.deltaTime;
        }

        //Tilt back to straight if no wall run

        if(!isWallLeft && !isWallRight)
        {
            if(wallRunCameraTilt > 0)
            {
                wallRunCameraTilt -= tiltSpeed * Time.deltaTime;
            }
            if(wallRunCameraTilt < 0)
            {
                wallRunCameraTilt += tiltSpeed * Time.deltaTime;
            }
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
    }

    void ChooseMoveSpeed()
    {
        moveSpeed = runForce;
    }

    void DoMovement()
    {
        if(horiz == 0 && verti == 0) return;
        Vector3 moveDirection = transform.right * horiz + transform.forward * verti;

        if(isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);
        }else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);
        }

    }

    void SpeedControl()
    {
        Vector3 movementVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(movementVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = movementVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void UpdatePlayerDrag()
    {
        if(isGrounded)
        {
            rb.drag = groundDrag;
        }else
        {
            rb.drag = airDrag;
        }
    }

    void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, transform.right, 1f, wallLayer);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, 1f, wallLayer);

        if(!isWallLeft && !isWallRight)
        {
            StopWallRun();
        }
    }

    void StartWallRun()
    {
        rb.useGravity = false;
        isWallRunning = true;

        if(rb.velocity.magnitude <= maxWallRunSpeed)
        {
            rb.AddForce(transform.forward * wallRunForce * Time.deltaTime, ForceMode.Force);
            

            //Make sure player sticks to wall
            if(isWallRight)
            {
                rb.AddForce(transform.right * wallRunForce / 5 * Time.deltaTime, ForceMode.Force);
            }
            if(isWallLeft)
            {
                rb.AddForce(-transform.right * wallRunForce / 5 * Time.deltaTime, ForceMode.Force);
            }
        }
    }

    void StopWallRun()
    {
        rb.useGravity = true;
        isWallRunning = false;
    }


    void Jump()
    {

        if(isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            isGrounded = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }else if(isWallRunning)
        {
            //Upward Walljump
            if(isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }

            //Sidewards Hop
            if(isWallRight && Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-transform.right * jumpForce * wallJumpMultiplier, ForceMode.Impulse);
                rb.AddForce(transform.up * jumpForce / 3, ForceMode.Impulse);
            }
            if(isWallLeft && Input.GetKey(KeyCode.D))
            {
                rb.AddForce(transform.right * jumpForce * wallJumpMultiplier, ForceMode.Impulse);
                rb.AddForce(transform.up * jumpForce / 3, ForceMode.Impulse);
            }
        }


    }

    // private void OnCollisionEnter(Collision other) 
    // {
    //     if(other.transform.CompareTag("Ground"))
    //     {
    //         isGrounded = true;
    //     }
    // }

    // private void OnCollisionExit(Collision other) 
    // {
    //     if(other.transform.CompareTag("Ground"))
    //     {
    //         isGrounded = false;
    //     }
    // }
}
