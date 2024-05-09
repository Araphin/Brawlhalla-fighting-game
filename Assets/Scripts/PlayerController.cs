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
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

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
    public float attackRange = 0.2f;
    public int attackDamage = 10;
    public float attackRate = 2f;
    float nextAttackTime = 0f;

    [Header("WeaponAnimators")]
    public RuntimeAnimatorController SwordController;
    public RuntimeAnimatorController SpearController;
    public RuntimeAnimatorController AxeController;
    public RuntimeAnimatorController HammerController;

    [Header("PlayerHealth")]
    public int maxHealth = 100;
    public int currentHealth;

    public int Lives = 3;
    Vector2 startPos;

    public float Knockback;
    public Vector2 PlayerDirection = Vector2.left;



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

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

    }


    public void GetInputs()
    {
        xAxis = Input.GetAxis("Horizontal");
    }

    void Flip()
    {

        PlayerDirection = PlayerDirection * -1;

        if (xAxis < 0)
        {
            PlayerDirection = Vector2.right;
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
        else if (xAxis > 0)
        {
            PlayerDirection = Vector2.left;
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
        else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
        {
            pState.jumping = true;

            airJumpCounter++;

            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }

        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
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
                attackDamage = 20;
                attackRange = 0.5f;
                attackRate = 2;
            }
            else if (weapon == "spear")
            {
                anim.runtimeAnimatorController = SpearController as RuntimeAnimatorController;
                attackDamage = 15;
                attackRange = 0.7f;
                attackRate = 2.2f;
            }
            // else if (weapon == "axe")
            //   {
            //      anim.runtimeAnimatorController = AxeController as RuntimeAnimatorController;
            //   }
            //  else if (weapon == "hammer")
            //   {
            //       anim.runtimeAnimatorController = HammerController as RuntimeAnimatorController;
            //    }


        }

        if (collision.CompareTag("Deathzone"))
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        anim.SetTrigger("Hurt");

        rb.AddForce(Vector2.right * 100);
        //  if (currentHealth <= 0)
        // {
        // Die2();
        // }
    }

    public void Die2()
    {
        Debug.Log("Player died!");

        anim.SetBool("IsDead", true);

       
        rb.velocityX = 0;
       
       
      //  this.enabled = false;
    }

    void Die()
    {
        if (Lives > 0)
        {
            Lives = Lives - 1;
            Respawn();
        }
        else if (Lives == 0) 
        { 
          Die2();
        }
    }

    void Respawn()
    { 
        
        transform.position = startPos;
    }


}
