using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public float maxHealth = 100;
    public float currentHealth;
    public Rigidbody2D rb;
    public float Knockback;
    public Vector2 PlayerDirection = Vector2.left;
    Transform playerTransform;

    [Header("Ground check settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask WhatisGround;

    private float xAxis;

    public int Lives = 3;
    Vector2 startPos;

    #region Public Variables
    public Transform rayCast;
    public LayerMask raycastMask;
    public float rayCastLength;
    public float attackDistance;
    public float moveSpeed;
    public float timer = 1;
    #endregion
    public Transform leftLimit;
    public Transform rightLimit;
    public TriggerArea triggerArea;

    #region Private Variables
    public RaycastHit2D hit;
    private Transform target;
    private Animator anim;
    private float distance;
    private bool AttackMode;
    public bool inRange;
    private float intTimer;
    #endregion

    [Header("Player Combat")]
    public Transform attackPoint;
    public LayerMask PlayerLayers;
    public float attackRange = 0.2f;
    public int attackDamage = 10;
    public float attackRate = 2f;
    public GameObject playerObject;

    public Vector2 EnemyDirection = Vector2.left;

    [Header("WeaponAnimators")]
    public RuntimeAnimatorController SwordController;
    public RuntimeAnimatorController SpearController;
    public RuntimeAnimatorController AxeController;
    public RuntimeAnimatorController HammerController;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

        playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        // Determine which direction the player is facing
        if (playerTransform.position.x > transform.position.x)
        {
            // Player is to the right of the enemy, so set PlayerDirection to left
            PlayerDirection = Vector2.left;
        }
        else
        {
            // Player is to the left of the enemy, so set PlayerDirection to right
            PlayerDirection = Vector2.right;
        }

        // Define the start and end values
        float startValue = 300f;
        float endValue = 75f;

        // Calculate the 't' value
        float t = (currentHealth / maxHealth);

        // Use Mathf.Lerp to calculate the knockback value
        float knockbackForce = Mathf.Lerp(startValue, endValue, t);

        // Print the knockback value for debugging
        Debug.Log("Knockback Force: " + knockbackForce);

        // Apply the knockback force
        rb.AddForce(PlayerDirection * knockbackForce, ForceMode2D.Impulse);

        // Check if the enemy should die
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Awake()
    {
        SelectTarget();
        intTimer = timer;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!AttackMode)
        {
            Move();
        }

        if (!InsideofLimits() && !inRange && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            SelectTarget();
        }
    }

    private void SelectTarget()
    {
        float distanceToLeft = Vector2.Distance(transform.position, leftLimit.position);
        float distanceToRight = Vector2.Distance(transform.position, rightLimit.position);

        if(distanceToLeft > distanceToRight)
        {
            target = leftLimit;
        }
        else
        {
            target = rightLimit;
        }

        Flip();
    }

    void OnTriggerEnter2D(Collider2D collision)   
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
            }

            if (collision.gameObject.tag == "Player")
            {
               target = collision.transform;
               inRange = true;
               Flip();
               
            }
 
            if (collision.CompareTag("Deathzone"))
            {
              Die();
            }
        } 

    void Move()
    {
        anim.SetBool("Running", true);
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName(""))
        {
            Vector2 targetPosition = new Vector2(target.position.x, transform.position.y);

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        }
    }

    public void StartAttacking()
    {
        if(AttackMode == false)
        {
            AttackMode = true;
            InvokeRepeating("Attack", 0f, 1f);
        }
        
    }

    void Attack()
    {
        print("Attacking");
        
        anim.SetBool("Running", false);
        anim.SetBool("Attack", true);
        //anim.SetTrigger("Attack");

        // Detect player in range of attack
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, PlayerLayers);

        // Damage them
        foreach (Collider2D player in hitPlayers)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage1(attackDamage, transform);
                print("Enemy hits Player: " + attackDamage);
            }
        }
        AttackMode=false;
    }

    public void StopAttack()
    {
        CancelInvoke("Attack");
        AttackMode = false;
        anim.SetBool("Attack", false);
        anim.SetBool("Running", true);
    }

    private void Start1()
    {
        startPos = transform.position;
    }

    
    public void Die2()
    {
        Debug.Log("Enemy died!");

        animator.SetBool("IsDead", true);

        //GetComponent<Collider2D>().enabled = false;

        this.enabled = false;
    }

    //private void Life()
    //{
    //    TextMeshPro Life = new TextMeshPro();
    //}

    void Die()
    {
        if (Lives > 0)
        {
            Lives = Lives - 1;
            Respawn();
            currentHealth = 100;
            Knockback = 0;
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

    private bool InsideofLimits()
    {
        return transform.position.x > leftLimit.position.x && transform.position.x < rightLimit.position.x;
    }

    //private void OnDrawGizmosSelected()
    //{
    //    float distanceToLeft = Vector2.Distance(transform.position, leftLimit.position);
    //    float distanceToRight = Vector2.Distance(transform.position, rightLimit.position);

    //    if(distanceToLeft > distanceToRight) 
    //    {
    //        target = leftLimit;
    //    }
    //    else
    //    {
    //        target = rightLimit;
    //    }

    //    Flip();

    //}

    private void Flip()
    {
        Vector3 rotation = transform.eulerAngles;
        if(transform.position.x > target.position.x)
        {
            rotation.y = 0f;
        }
        else
        {
            rotation.y = 180f;
        }

        transform.eulerAngles = rotation;
    }

    
} 
