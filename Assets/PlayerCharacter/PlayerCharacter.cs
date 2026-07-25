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

    //Gravity
    public float gravityScale = 1f;

    //Direction
    private bool isFacingRight    = true;

    //WallSliding
    public  bool isWallSliding    = false;
    public float wallSlidingSpeed = 2f;

    //Moving
    public float moveDirection;
    public  bool isGrounded     = true;
    public float moveSpeed      = 5f;


    //Jumping
    public float jumpStrength   = 10f;
    public int   maxJumps     = 3;
    private int  currNumJumps;

    //Dashing
    public float dashStrength   = 12f;
    private int currNumDashes;
    public  int maxDashes = 1;
    public float dashCooldown   = 0.5f;
    public float dashDuration   = 0.3f;
    public bool dashIsOnCooldown= false;
    public bool isDashing       = false;
    public bool dashFinished    = false;


    //WallJumping
    public Vector2 wallJumpStrength  = new(7f, 7f);
    public bool wallJumpIsOnCooldown = false;
    public float  wallJumpCooldown   = 0.5f;
    public float wallJumpDuration    = 0.15f;

    //Checks
    public Transform wallCheck;
    public Transform groundCheck;


    //Movement Block
    public bool movementBlocked = false;

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
        SlowDownXAfterDash();
        TurnOffGravity(isDashing);
        ReplenishDashesAndJumps();
    }



    //TODO: Handle Airborne movement better
    public void Move()
    {
        Vector2 targetMovementVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocityY);
        FlipHorizontal();
        moveDirection = move.action.ReadValue<float>();
        if(!movementBlocked && isGrounded)
            rb.linearVelocity = targetMovementVelocity;
        else if(!movementBlocked && !isGrounded)
            if(moveDirection != 0)
                rb.linearVelocity = targetMovementVelocity;

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
    }    private bool CheckOnWall()
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

    //------------------------------------------------------------------------//
    //                               Helpers
    //------------------------------------------------------------------------//
    private IEnumerator SetFlagTrueForDuration(Action<bool> setBool, float duration)
    {
        setBool(true);
        yield return new WaitForSeconds(duration);
        setBool(false);
    }

    private IEnumerator DoAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }


    public void BlockMovement(float duration)
    {
        StartCoroutine(SetFlagTrueForDuration(value => movementBlocked = value, duration));
    }


    [ContextMenu("respawn")]
    public void RespawnCurrCheckpoint()
    {
        logic.RespawnCurrCheckpoint();
    }

    public void ReplenishDashesAndJumps()
    {
        if(isGrounded){
            currNumDashes = maxDashes;
            currNumJumps  = maxJumps;
        }

    }


    //------------------------------------------------------------------------//
    //                      WALL JUMPING MECHANICS
    //------------------------------------------------------------------------//
        public void PutWallJumpOnCooldown(float duration)
    {
        StartCoroutine(SetFlagTrueForDuration(value => wallJumpIsOnCooldown= value, duration));
    }

    private void WallJump(int wallJumpDirection)
    {
        if(!wallJumpIsOnCooldown)
        {
            //put walljump on cooldown
            isWallSliding        = false;
            wallJumpIsOnCooldown = true;

            PutWallJumpOnCooldown(wallJumpCooldown);
            //Restrict movement while walljumping is generating momentum
            BlockMovement(wallJumpDuration);

            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpStrength.x, wallJumpStrength.y);

        }
    }

    //------------------------------------------------------------------------//
    //                      DASH MECHANICS
    //------------------------------------------------------------------------//

    //TODO: Implement dash recharge on grounded
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currNumDashes > 0){
                int dashDirection = isFacingRight? 1: -1;
                DashHelper(dashDirection);
                currNumDashes --;
            }
        }
    }

    public void PutDashOnCooldown(float duration)
    {
        StartCoroutine(SetFlagTrueForDuration(value => dashIsOnCooldown= value, duration));
    }

    public void SetIsDashing(float duration)
    {
        StartCoroutine(SetFlagTrueForDuration(value => isDashing= value, duration));
    }

    public void RaiseDashFinishedAfter(float duration)
    {
        StartCoroutine(DoAfterDelay(duration, () => dashFinished = true));
    }

    private void DashHelper(int dashDirection)
    {
        if (!dashIsOnCooldown)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashStrength, rb.linearVelocityY);

            BlockMovement(dashDuration);

            PutDashOnCooldown     (dashCooldown);
            SetIsDashing          (dashDuration);
            RaiseDashFinishedAfter(dashDuration);

        }
    }

    private void TurnOffGravity(bool isOff)
    {
        if(isOff)
            rb.gravityScale = 0;
        else
            rb.gravityScale = gravityScale;
    }

    private void SlowDownXAfterDash()
    {
        if (dashFinished){
            rb.linearVelocityX = (isFacingRight? 1 : -1) * moveSpeed;
            dashFinished = false;
        }
    }

    //------------------------------------------------------------------------//
    //                      JUMP MECHANICS
    //------------------------------------------------------------------------//

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if ((isGrounded || (currNumJumps > 0)) && !isWallSliding)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpStrength);
                currNumJumps --;
            }
            if(!isGrounded && isWallSliding)
            {
                int wallJumpDirection = isFacingRight?-1:1;
                WallJump(wallJumpDirection);
            }
        }

        //Enable shorter jumps
        if (context.canceled && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
        }
    }


    //------------------------------------------------------------------------//
    //                      COLLISION MECHANICS
    //------------------------------------------------------------------------//
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

}
