using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Behaviour : MonoBehaviour
{
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

    //public Transform 
    //constantly follow player
    //Stop in attack range
    //Attack player
    //jump around at right time
    //recover away from edge
    //run towards weapon spawn

    private void Awake()
    {
        intTimer = timer;
        anim = GetComponent<Animator>();

    }

    void Update()
    {
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
                anim.SetBool("canWalk", false);
                StopAttack();
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
        }
    }

    private void OnTriggerEnter2D(Collider2D trig)
    {
        if (trig.gameObject.tag == "Player")
        {
            target = trig.gameObject;
            inRange = true;
        }
    }

    void Move()
    {
        anim.SetBool("Running", true);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(""))
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
    }

    void Cooldown()
    {
        timer -= Time.deltaTime;

        if(timer <= 0 && cooling && AttackMode)
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
    }
    void RaycastDebugger()
    {
        if (distance > attackDistance)
        {
            Debug.DrawRay(rayCast.position, Vector2.left * rayCastLength, Color.green);
        }
    }

    public void TriggerCooling()
    {
        cooling = true;
    }


}
