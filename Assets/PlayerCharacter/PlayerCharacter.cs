using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField]private Collider2D mainCollider;
    [SerializeField] private InputActionReference move;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private Logic logic;

    // Do not check if grounded while the character is in the "stateCheckInterval" of the jump (leaving the ground but the raycast can still hit the gorund)
    public float stateCheckInterval = 0.2f;

    public bool isGrounded          = true;
    public bool isOnWall            = false;
    public bool isOnWallLeft        = false;
    public bool isOnWallRight       = false;
    public bool isCheckingOnWall    = true;
    public bool isFacingRight       = true;
    public bool isCheckingGrounded  = true;
    public bool isJumping;
    public bool isDashing;
    public bool checkOnWall         = true;

    public bool canMove = true;
    public bool canJump = true;
    public bool canDash = true;


    public float moveDirection;
    public float moveSpeed      = 5f;
    public float jumpStrength   = 10f;
    public int defNumOfJumps     = 2;
    public int currNumOfJumps;
    public float dashStrength   = 12f;
    public int defNumOfDashes   = 1;
    public int currNumOfDashes;
    public float dashDuration   = 0.1f;
    public float dashCooldown   = 0.5f;
    public float onWallGravity  = 1;
    public float defaultGravity = 3;






    public void Start()
    {
        currNumOfJumps   = defNumOfJumps;
        currNumOfDashes  = defNumOfDashes;
        logic            = GameObject.FindGameObjectWithTag("Logic").GetComponent<Logic>();
    }

    public void Awake(){
        StartCoroutine(CheckGroundedRoutine());
        StartCoroutine(CheckIsOnWallRoutine());
    }

    public void FixedUpdate()
    {
        if(canMove){
            moveDirection = move.action.ReadValue<float>();
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocityY);
        }

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

        if (isOnWall)
        {
            rb.gravityScale = onWallGravity;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }

        if(rb.linearVelocityY != 0) isJumping = true;


        if (!isCheckingGrounded) StartCoroutine(CheckGroundedRoutine());
        if (!isCheckingOnWall) StartCoroutine(CheckIsOnWallRoutine());
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && canJump)
        {
            if (currNumOfJumps > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpStrength);
                currNumOfJumps --;
            }
        }
        else if(context.performed && isOnWall)
        {
            isOnWall = false;
            StartCoroutine(WallJumpRoutine());
        }

        if (context.canceled && !isOnWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
        }
    }
    //TODO: Make a wall collision stop the dash
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
           StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        if (currNumOfDashes > 0)
        {
            // Program dashes from netural position
            if(moveDirection == 0 && canDash){
                if(isFacingRight)
                    moveDirection = 1;
                else
                    moveDirection = -1;
            }

            float originalGravity = rb.gravityScale;

            isDashing = true;
            canDash   = false;
            canMove   = false;
            canJump   = false;

            rb.gravityScale = 0f;

            if(!isGrounded) currNumOfDashes --;

            print("isDashing");

            rb.linearVelocity = new Vector2(dashStrength * moveDirection, 0);
            yield return new WaitForSeconds(dashDuration);

            isDashing = false;
            canMove   = true;
            canJump   = true;

            rb.gravityScale = originalGravity;

            yield return new WaitForSeconds(dashCooldown);

            canDash   = true;
        }
    }

    IEnumerator WallJumpRoutine()
    {
        canDash   = false;
        canMove   = false;
        canJump   = false;
        isOnWall  = false;

        print("isWallJumping");
        rb.gravityScale = 0;
        if(isOnWallLeft) rb.linearVelocity = new Vector2(jumpStrength * 0.707f, jumpStrength * 0.707f);
        if(isOnWallRight) rb.linearVelocity = new Vector2(-jumpStrength * 0.707f, jumpStrength * 0.707f);
        checkOnWall = false;

        yield return new WaitForSeconds(dashDuration);

        canDash   = true;
        canMove   = true;
        canJump   = true;
        rb.gravityScale = defaultGravity;


        yield return new WaitForSeconds(dashCooldown);
        checkOnWall = true;
    }

    IEnumerator CheckGroundedRoutine()
    {
        isCheckingGrounded = true;
        RaycastHit2D boxcast = Physics2D.BoxCast(GetComponent<Collider2D>().bounds.center, GetComponent<Collider2D>().bounds.size, 0, Vector2.down, 0.1f, GroundLayer);
        isGrounded = boxcast;

        if(isGrounded){
            isJumping = false;
            currNumOfJumps = defNumOfJumps;
            currNumOfDashes = defNumOfDashes;
        }
        yield return new WaitForSeconds(stateCheckInterval);
        isCheckingGrounded = false;
    }

    IEnumerator CheckIsOnWallRoutine()
    {
        if(checkOnWall){
            isCheckingOnWall = true;
            Vector2 smallerBox = new Vector2(GetComponent<Collider2D>().bounds.size[0], GetComponent<Collider2D>().bounds.size[1] * 0.2f); // Avoid clinging to walls with extremities
            RaycastHit2D checkOnWallRight = Physics2D.BoxCast(GetComponent<Collider2D>().bounds.center, smallerBox, 0, Vector2.right, 0.01f, GroundLayer);
            RaycastHit2D checkOnWallLeft  = Physics2D.BoxCast(GetComponent<Collider2D>().bounds.center, smallerBox, 0, Vector2.left, 0.01f, GroundLayer);
            isOnWallRight = checkOnWallRight  & !isGrounded;
            isOnWallLeft  = checkOnWallLeft   & !isGrounded;
            isOnWall = (isOnWallRight & (moveDirection == 1)) || (isOnWallLeft & (moveDirection == -1));

            canJump = isOnWall? false: true;
            if(isJumping & isOnWall)
            {
                isJumping = false;
                rb.linearVelocityY = 0;
            }
            yield return new WaitForSeconds(stateCheckInterval);
            isCheckingOnWall = false;
        }
        else
        {
            isOnWall = false;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Damage")
        {
            print("Damage");
        }
    }

     private void OnTriggerEnter2D(Collider2D collision) {
        print("Spawn changed");
        logic.SetCheckPoint(collision.transform.position);
    }
}
