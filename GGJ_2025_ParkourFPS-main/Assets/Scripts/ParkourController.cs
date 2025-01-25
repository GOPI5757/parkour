using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class ParkourController : MonoBehaviour
{

    [Header("Camera")]
    public float mouseSensitivity;
    public GameObject attachedCam;
    public float tiltSpeed;

    [Header("Movement")]
    public TMP_Text speedText;
    public float runForce;
    public float maxRunSpeed;
    public float groundDrag;
    public float airDrag;
    public float slideDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;

    //GameObject currentGroundObject;


    [Header("Jumping")]
    public float jumpForce;
    public float wallJumpMultiplier;
    public float airMultiplier;

    [Header("Dashing")]
    public float dashForce;
    public float maxDashSpeed;
    public float dashLifeTime;
    public float dashCoolDown;

    bool dashing;
    bool canDash;
    float dashTimer = 0f;

    [Header("Sliding")]
    public float slideForce;
    public float maxSlideTime;
    public float maxSlideSpeed;
    public float maxSlopeAngle;
    public float slideYScale;

    [Header("Sounds")]

    public AudioSource audioSource;

    public AudioClip jumpSound;
    public AudioClip dashSound;

    bool sliding;
    float slideTimer;
    float startYScale;

    RaycastHit slopeHit;

    [Header("Checkpoint Control")]
    public LayerMask checkPointLayer;

    [Header("Wallrunning")]
    public LayerMask wallLayer;
    public float wallRunForce, maxWallRunTime, maxWallRunSpeed;
    public float maxWallRunTilt;

    float wallRunCameraTilt;
    float maxSpeed;

    bool isWallLeft, isWallRight;
    bool isWallRunning;

    bool isGrounded;

    Rigidbody rb;
    Animator camAnim;

    bool cameraLocked = false;

    float horiz, verti;
    float mouseX, mouseY;
    float moveForce;
    float yRot, xRot;

    void Start()
    {
        moveForce = runForce;
        canDash = true;
        rb = GetComponent<Rigidbody>();
        camAnim = attachedCam.GetComponent<Animator>();
        Application.targetFrameRate = 60;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CheckForInput();
        CheckWallInput();
        CheckSlideInput();
        CheckForWall();
        GroundCheck();
        ChooseMoveSpeed();
        ChooseMaxSpeed();
        PlayerLook();
        UpdatePlayerDrag();

        Vector3 movementVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        speedText.text = $"Speed: {(int)movementVel.magnitude}";
    }

    void FixedUpdate()
    {
        DoMovement();
        Dashing();
        Sliding();
        SpeedControl();
    }

    void CheckForInput()
    {
        horiz = Input.GetAxisRaw("Horizontal");
        verti = Input.GetAxisRaw("Vertical");

        mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            Dash();
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

    void CheckSlideInput()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl) && (horiz != 0 || verti != 0))
        {
            StartSlide();
        }

        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopSlide();
        }
    }

    void PlayASound(AudioClip clip)
    {
        audioSource.Stop();

        audioSource.clip = clip;
        audioSource.Play();
    }

    void EnableAnimAndLockCam()
    {
        camAnim.enabled = true;
        cameraLocked = true;
    }

    void DisableAnimAndLockCam()
    {
        camAnim.enabled = false;
        cameraLocked = false;
    }

    void PlayerLook()
    {
        yRot += mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        if(!cameraLocked)
        {
            attachedCam.transform.localRotation = Quaternion.Euler(Mathf.Round(xRot * 10f) / 10f, 0f, wallRunCameraTilt);
        }
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
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.2f, groundLayer) ||
            Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.2f, checkPointLayer);

        //currentGroundObject = hit.transform.gameObject;
    }

    

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.2f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    void ChooseMoveSpeed()
    {
        moveForce = runForce;
    }

    void ChooseMaxSpeed()
    {
        if(sliding)
        {
            maxSpeed = maxSlideSpeed;
        }
        else if(dashing)
        {
            maxSpeed = maxDashSpeed;
        }else
        {
            maxSpeed = maxRunSpeed;
        }
    }

    void DoMovement()
    {
        if(horiz == 0 && verti == 0) return;
        Vector3 moveDirection = transform.right * horiz + transform.forward * verti;

        if(isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveForce, ForceMode.Force);
        }else
        {
            rb.AddForce(moveDirection.normalized * moveForce * airMultiplier, ForceMode.Force);
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
        if(sliding)
        {
            rb.drag = slideDrag;
        }
        else if(isGrounded)
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
            PlayASound(jumpSound);
        }else if(isWallRunning)
        {
            //Upward Walljump
            if(isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                PlayASound(jumpSound);
            }

            //Sidewards Hop
            if(isWallRight && Input.GetKey(KeyCode.A))
            {
                rb.AddForce(-transform.right * jumpForce * wallJumpMultiplier, ForceMode.Impulse);
                rb.AddForce(transform.up * jumpForce / 3, ForceMode.Impulse);
                PlayASound(jumpSound);
            }
            if(isWallLeft && Input.GetKey(KeyCode.D))
            {
                rb.AddForce(transform.right * jumpForce * wallJumpMultiplier, ForceMode.Impulse);
                rb.AddForce(transform.up * jumpForce / 3, ForceMode.Impulse);
                PlayASound(jumpSound);
            }
        }


    }

    void Dash()
    {
        if(dashing || !canDash) return;

        dashing = true;
        canDash = false;
        dashTimer = dashLifeTime;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        PlayASound(dashSound);
    }

    void Dashing()
    {
        if(!dashing) return;
        rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);

        dashTimer -= Time.deltaTime;
        if(dashTimer <= 0f)
        {
            dashing = false;
            StartCoroutine(DashCooldown());
        }
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCoolDown);
        canDash = true;
    }

    void StartSlide()
    {
        if(sliding || !isGrounded) return;
        sliding = true;
        EnableAnimAndLockCam();

        if(OnSlope())
        {
            camAnim.SetBool("isSlope", true);
        }else
        {
            camAnim.SetBool("slide", true);
        }
        startYScale = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector2.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    void Sliding()
    {
        if(!sliding) return;


        //Sliding on straight ground
        if(!OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(transform.forward * slideForce, ForceMode.Acceleration);

            slideTimer -= Time.deltaTime;
        }

        //Sliding on slopes
        else
        {
            rb.AddForce(GetSlopeMoveDirection(transform.forward) * slideForce, ForceMode.Acceleration);
        }
    
        if(slideTimer <= 0f)
        {
            StopSlide();
        }
    }

    void StopSlide()
    {
        if(!sliding) return;


        sliding = false;
        camAnim.SetBool("isSlope", false);
        camAnim.SetBool("slide", false);

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    public void SlideAnimFinished()
    {
        DisableAnimAndLockCam();
    }

    SaveDatas GetSaveData()
    {
        return SaveSystem.LoadData() == null ? new SaveDatas() : SaveSystem.LoadData();
    }

    private void OnCollisionEnter(Collision other) 
    {
        if(other.transform.CompareTag("Bubble"))
        {
            if(dashing)
            {
                other.transform.GetComponent<Bubble>().DestroyBubble();
            }
        }
    }

    // private void OnCollisionExit(Collision other) 
    // {
    //     if(other.transform.CompareTag("Ground"))
    //     {
    //         isGrounded = false;
    //     }
    // }
}
