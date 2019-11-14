using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;


    Animator anim;
    GameObject player;
    private PlayerHealth _playerHealth;
    EnemyHealth enemyHealth;
    bool playerInRange;
    float timer;


    void Awake ()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        anim = GetComponent <Animator> ();
    }


    void OnTriggerEnter (Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = true;
        }
    }


    void OnTriggerExit (Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = false;
        }
    }


    void Update ()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag ("Player");
            _playerHealth = player.GetComponent<PlayerHealth>();
        }
        timer += Time.deltaTime;

        if(timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0)
        {
            Attack ();
        }

//        if(playerHealth.currentHealth <= 0)
//        {
//            anim.SetTrigger ("PlayerDead");
//        }
    }


    void Attack ()
    {
        timer = 0f;
        
        

        if(_playerHealth.currentHealth > 0)
        {
            _playerHealth.TakeDamage (attackDamage);
        }
    }
}
