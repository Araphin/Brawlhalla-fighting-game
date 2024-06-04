using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public bool Triggered = false;
    public Enemy enemy;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Triggered = true;
            enemy.StartAttacking();
            //print("Player triggered enemy");
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Triggered = false;
            enemy.StopAttack();
            //print("Player moved away");
        }
    }
}
