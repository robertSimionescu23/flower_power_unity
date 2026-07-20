using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputActionReference move;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private Logic logic;
    public bool isGrounded          = true;
    public bool isWallSliding       = false;
    private bool isFacingRight       = true;
    public bool isDashing;
    public float wallSlidingSpeed = 2f;
    public float gravityScale = 1f;


    public float moveDirection;
    public float moveSpeed      = 5f;
    public float jumpStrength   = 10f;
    public int maxJumps     = 3;
    private int currNumJumps;
    public float dashStrength   = 12f;
    public float dashDuration   = 0.3f;
    private float currDashDuration = 0.1f;
    private float currDashCooldown   = 0f;
    public float dashCooldown   = 0.5f;
    private float dashDirection = 1;
    public bool dashIsOnCooldown = false;

    public float wallJumpDuration = 0.1f;
    private float currWallJumpDuration = 0.1f;
    public bool  isWallJumping     =false;
    private float wallJumpDirection = 1;
    public Vector2 wallJumpStrength = new(5f, 5f);


    public Transform wallCheck;
    public Transform groundCheck;


    public void Start()
    {
        logic            = GameObject.FindGameObjectWithTag("Logic").GetComponent<Logic>();
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravityScale;
    }

    public void Update()
    {
        isGrounded    = CheckGrounded();
        isWallSliding = CheckOnWall();

        Move();
        WallSlide();
        WallJump();
        DashUpdate();
    }

    public void Move()
    {
        FlipHorizontal();
        moveDirection = move.action.ReadValue<float>();
        if(!isWallJumping && !isDashing)
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocityY);
    }

    private void FlipHorizontal()
    {
        if (rb.linearVelocityX > 0.01f)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            isFacingRight = true;
        }
        else if (rb.linearVelocityX < -0.01f)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            isFacingRight = false;
        }
    }

    private bool CheckGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.05f, GroundLayer);
    }
    private bool CheckOnWall()
    {
        return !isGrounded && Physics2D.OverlapCircle(wallCheck.position, 0.05f, GroundLayer);
    }

    private void WallSlide()
    {
        if(isWallSliding && !isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    private void WallJump()
    {
        if((currWallJumpDuration > 0f) && isWallJumping)
        {
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpStrength.x, wallJumpStrength.y);
            currWallJumpDuration -= Time.deltaTime;
        }
        else
        {
            isWallJumping = false;
            currWallJumpDuration = wallJumpDuration;
        }
    }

    private void DashUpdate()
    {
        if((currDashDuration > 0f) && isDashing && !dashIsOnCooldown)
        {
            rb.linearVelocity  = new Vector2(dashDirection * dashStrength, rb.linearVelocityY);
            currDashDuration  -= Time.deltaTime;
            currDashCooldown   = dashCooldown;
            dashIsOnCooldown   = true;
            rb.gravityScale    = 0;
        }
        else
        {
            currDashDuration = dashDuration;
            rb.gravityScale = gravityScale;
            isDashing = false;
        }

        if((currDashCooldown > 0f) && dashIsOnCooldown && !isDashing)
        {
            currDashCooldown -= Time.deltaTime;
        }
        else
        {
            dashIsOnCooldown = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(isGrounded && !context.performed)
            currNumJumps = maxJumps;

        if (context.performed)
        {
            if ((isGrounded || (currNumJumps > 0)) && !isWallSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpStrength);
                currNumJumps --;
            }
            if(!isGrounded && isWallSliding)
            {
                isWallSliding = false;
                isWallJumping = true ;
                wallJumpDirection = isFacingRight?-1:1;
            }
        }

        //Enable shorter jumps
        if (context.canceled && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
        }
    }
    //TODO: Implement dash recharge on grounded
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isDashing = true;
            dashDirection = isFacingRight? 1: -1;
        }
    }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
           logic.RespawnCurrCheckpoint();
        }
    }

     private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            if ((Vector3)logic.currCheckpoint != collision.transform.position)
                print("Spawn changed " + collision.transform.position);
                logic.SetCheckPoint(collision.transform.position);
        }

        if (collision.gameObject.CompareTag("NextRoom"))
        {
            NextRoomDoor door = collision.GetComponent<NextRoomDoor>();
            logic.ChangeRoom(door.destinationRoom, door);
        }
    }


    [ContextMenu("respawn")]
    public void RespawnCurrCheckpoint()
    {
        logic.RespawnCurrCheckpoint();
    }
}
