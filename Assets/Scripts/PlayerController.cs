using NUnit.Framework.Internal.Execution;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal movement settings")]
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float jumpForce = 45;
    private int jumpBufferCounter;
    [SerializeField] private int jumpBufferFrames = 0;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;

    [Header("Ground check settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask WhatisGround;


    PlayerStateList pState;
    private Rigidbody2D rb;
    private float xAxis;
    public Animator anim;

    [Header("Player Combat")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public float attackRange = 0.5f;
    public int attackDamage = 40;
    public float attackRate = 2f;
    float nextAttackTime = 0f;

    [Header("WeaponAnimators")]
    public RuntimeAnimatorController SwordController;
    public RuntimeAnimatorController SpearController;
    public RuntimeAnimatorController AxeController;
    public RuntimeAnimatorController HammerController;





    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();
        Flip();
        Move();
        Jump();

        if(Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
      
    }

    void GetInputs()
    {
        xAxis = Input.GetAxis("Horizontal");
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
    }




    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Running", rb.velocity.x != 0 && Grounded());
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, WhatisGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, WhatisGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, WhatisGround))

        {
            return true;
        }
        else
        {
            return false;
        }
    }



    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);

            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);

                pState.jumping = true;
            }
        }

        if (Input.GetButtonDown("Jump") && Grounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            pState.jumping = true;
        }

        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }


        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }



    void Attack()
    {
        //Play an attack animations
        anim.SetTrigger("Attack");

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);


        // damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();
            if (newEnemy.currentHealth > 0)
            {
               newEnemy.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
           return;
        

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

   

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "pickup")
        {
            Weapons pickup = collision.gameObject.GetComponent<Weapons>();
            string weapon = pickup.randomWeapon();
            if (weapon == "sword")
            {
                anim.runtimeAnimatorController = SwordController as RuntimeAnimatorController;
            }
            else if (weapon == "spear")
            {
                anim.runtimeAnimatorController = SpearController as RuntimeAnimatorController;
            }
            else if (weapon == "axe")
            {
                anim.runtimeAnimatorController = AxeController as RuntimeAnimatorController;
            }
            else if (weapon == "hammer")
            {
                anim.runtimeAnimatorController = HammerController as RuntimeAnimatorController;
            }

        }
    }
}