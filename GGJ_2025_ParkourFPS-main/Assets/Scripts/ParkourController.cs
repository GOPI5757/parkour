using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

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
    public float slopeRotSpeed;
    public float maxSlopeAngle;
    public float slideYScale;

    [Header("Sounds")]

    public AudioSource audioSource;

    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip keySound;

    bool sliding, autoSliding;
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

    [Header("Button Images")]

    [SerializeField]
    GameObject SpaceBar_img;

    [SerializeField]
    GameObject lctrl_img;

    [SerializeField]
    GameObject lshift_img;

    [Header("Other")]

    [SerializeField]
    public bool HasKey = false;

    [SerializeField]
    public GameObject key_tick_img;

    [Header("Time")]

    [SerializeField]
    float Seconds = 0f;

    [SerializeField]
    int minutes = 0;

    [SerializeField]
    TMP_Text TimeTxt;

    public static ParkourController instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

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
        RotateToGround();
        ChooseMoveSpeed();
        ChooseMaxSpeed();
        PlayerLook();
        UpdatePlayerDrag();
        SetButtonAnimations();
        TimeSetup();

        if (HasKey)
        {
            key_tick_img.SetActive(true);
        }

        Vector3 movementVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        speedText.text = $"Speed: {(int)movementVel.magnitude}";
    }

    void FixedUpdate()
    {
        DoMovement();
        Dashing();
        Sliding();
        AutoSlopeSliding();
        SpeedControl();
    }

    void SetButtonAnimations()
    {
        if (isGrounded)
        {
            SpaceBar_img.GetComponent<Animator>().SetBool("HasClicked", false);
        }
        else
        {
            SpaceBar_img.GetComponent<Animator>().SetBool("HasClicked", true);
        }

        if(!sliding)
        {
            lctrl_img.GetComponent<Animator>().SetBool("HasClicked", false);
        } else
        {
            lctrl_img.GetComponent<Animator>().SetBool("HasClicked", true);
        }

        if(!dashing)
        {
            lshift_img.GetComponent<Animator>().SetBool("HasClicked", false);
        } else
        {
            lshift_img.GetComponent<Animator>().SetBool("HasClicked", true);
        }
    }

    void TimeSetup()
    {
        Seconds += Time.deltaTime;
        if(Seconds >= 60f)
        {
            Seconds = 0f;
            minutes++;
        }
        TimeTxt.text = $"{minutes.ToString("00")}:{((int)Seconds).ToString("00")}";
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

        if(OnSlope())
        {
            StartAutoSlide();
        }else if(!sliding && autoSliding)
        {
            Debug.Log("Slope stopped");
            StopSlide();
        }
    }

    public void PlayASound(AudioClip clip)
    {
        audioSource.Stop();

        audioSource.clip = clip;
        audioSource.Play();
    }

    void EnableAnimAndLockCam()
    {
        camAnim.enabled = true;
        camAnim.Rebind();
        camAnim.Update(0f);
        cameraLocked = true;
    }

    void DisableAnimAndLockCam()
    {
        xRot = 0f;
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.2f, groundLayer);

        //currentGroundObject = hit.transform.gameObject;
    }
    

    void RotateToGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.2f, groundLayer))
        {
            // Get the slope normal
            Vector3 slopeNormal = slopeHit.normal;

            // Calculate the target rotation
            Quaternion slopeRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, slopeNormal), slopeNormal);

            Vector3 targetEulerAngles = slopeRotation.eulerAngles;
            targetEulerAngles.y = transform.eulerAngles.y; // Keep current Y rotation
            targetEulerAngles.z = 0; 

            // Smoothly rotate the player towards the slope
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetEulerAngles), Time.deltaTime * slopeRotSpeed);
        }
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

    public Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
    }

    void ChooseMoveSpeed()
    {
        moveForce = runForce;
    }

    void ChooseMaxSpeed()
    {
        if(sliding || autoSliding)
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
        if(sliding || !isGrounded || OnSlope() || autoSliding) return;
        sliding = true;
        EnableAnimAndLockCam();

        camAnim.SetBool("slide", true);
        startYScale = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector2.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    void Sliding()
    {
        if(!sliding || autoSliding) return;


        //Sliding on straight ground
        if(!OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(transform.forward * slideForce, ForceMode.Acceleration);

            slideTimer -= Time.deltaTime;
        }
    
        if(slideTimer <= 0f)
        {
            StopSlide();
        }
    }

    void StartAutoSlide()
    {
        if(autoSliding || !isGrounded || sliding) return;
        autoSliding = true;
        //EnableAnimAndLockCam();

        //camAnim.SetBool("isSlope", true);

        startYScale = transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector2.down * 5f, ForceMode.Impulse);

    }

    void AutoSlopeSliding()
    {
        if(!autoSliding) return;
        rb.AddForce(GetSlopeMoveDirection() * slideForce, ForceMode.Acceleration);
    }
    

    void StopSlide()
    {
        if(!sliding && !autoSliding) return;


        sliding = false;
        autoSliding = false;
        camAnim.SetBool("isSlope", false);
        camAnim.SetBool("slide", false);

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
    

    public void SlideAnimFinished()
    {
        Debug.Log("Slide completed!");
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

        if(other.transform.CompareTag("FinishLine"))
        {
            SceneManager.LoadScene(2);
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
