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
    public float timer;
    #endregion

    #region Private Variables
    public RaycastHit2D hit;
    private GameObject target;
    private Animator anim;
    private float distance;
    private bool AttackMode;
    public bool inRange;
    private bool cooling;
    private float intTimer;
    #endregion

    [Header("Player Combat")]
    public Transform attackPoint;
    public LayerMask PlayerLayers;
    public float attackRange = 0.2f;
    public int attackDamage = 10;
    public float attackRate = 2f;
    float nextAttackTime = 0f;
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
        //else if (Die2())
        //{
           
        //}
        // Define the start and end values
        float startValue = 300f;
        float endValue = 75f;        

        // Calculate the 't' value
        float t = (currentHealth / maxHealth);

        print(t);

        // Use Mathf.Lerp to calculate the knockback value
        float Knockback = Mathf.Lerp(startValue, endValue, t);

        // Print the knockback value
        print(Knockback);
    

        rb.AddForce(PlayerDirection * Knockback);
       
       // if (currentHealth <= 0)
      //  { 
      //     Die();
     //   }  
    }

    //void Die()
   // {
   //     Debug.Log("Enemy died!");

   //     animator.SetBool("IsDead", true);

        //GetComponent<Collider2D>().enabled = false;
   //     this.enabled = false;
        
    //}

   
    
     

        private void Awake()
        {
            intTimer = timer;
            anim = GetComponent<Animator>();

        }

        void Update()
        {
            if (Time.time >= nextAttackTime)
            {
                 if (AttackMode)
                 {
                     Attack();
                     nextAttackTime = Time.time + 1f / attackRate;
                 }
            }

            if (inRange)
            {
                hit = Physics2D.Raycast(rayCast.position, Vector2.left, rayCastLength, raycastMask);
                RaycastDebugger();

                if (hit.collider != null)
                {
                    EnemyLogic();
                }
                else if (hit.collider == null)
                {
                    inRange = false;
                }

                if (inRange == false)
                {
                    anim.SetBool("Running", false);
                    StopAttack();
                }

               
            }
        }

    void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.transform.position);

        if (distance > attackDistance)
        {
            Move();
            StopAttack();
        }
        else if (attackDistance >= distance && cooling == false)
        {
            Attack();
        }

        if (cooling)
        {
            Cooldown();
            anim.SetBool("Attack", false);
        }
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
        }

        if (collision.gameObject.tag == "Player")
            {
               target = collision.gameObject;
               inRange = true;
               Attack();
               print ("HitPlayer");
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
                Vector2 targetPosition = new Vector2(target.transform.position.x, transform.position.y);

                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            }
        }

        void Attack()
        {
            
            timer = intTimer;
            AttackMode = true;

            anim.SetBool("Running", false);
            anim.SetBool("Attack", true);

            //Play an attack animations
             anim.SetTrigger("Attack");

        // Detect enemies in range of attack
        //Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, PlayerLayers);


        //// damage them
        //foreach (Collider2D player in hitPlayer)
        //{
        // //check for player controller script
        //    PlayerController newPlayer = player.GetComponent<PlayerController>();
        //    if (newPlayer.currentHealth > 0)
        //    {
        //        newPlayer.TakeDamage1(attackDamage);
        //        print("attack");
        //    }
        //}

           playerObject.GetComponent<PlayerController>().TakeDamage1(attackDamage);
    }

        void Cooldown()
        {
            timer -= Time.deltaTime;

            if (timer <= 0 && cooling && AttackMode)
            {
                cooling = false;
                timer = intTimer;
            }
        }

        void StopAttack()
        {
            cooling = false;
            AttackMode = false;
            anim.SetBool("Attack", false);
            anim.SetBool("Running", true); 
        }

        void RaycastDebugger ()
        {
            if(distance > attackDistance)
            {
                Debug.DrawRay(rayCast.position, Vector2.left* rayCastLength, Color.green);
            }
        }
    

        void Flip()
        {
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
