using UnityEngine;
using System.Collections.Generic;

public class EnemyAttack : MonoBehaviour
{

    public static Engine engine; 
    
    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;


    Animator anim;
    private List<PlayerHealth> _playerHealths = new List<PlayerHealth>();
    private List<GameObject> players = new List<GameObject>();
    EnemyHealth enemyHealth;
    PlayerHealth playerInRange;
    bool isPlayerInRange;
    float timer;


    void Awake ()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        anim = GetComponent <Animator> ();
        _playerHealths = engine.server.playersHealth;
        players = engine.server.playersObjects;
    }


    void OnTriggerEnter (Collider other)
    {
        if (!players.Contains(other.gameObject)) return;
        
        isPlayerInRange = true;
        var index = players.IndexOf(other.gameObject);
        playerInRange = _playerHealths[index];
    }


    void OnTriggerExit (Collider other)
    {
        if (!players.Contains(other.gameObject)) return;
        
        isPlayerInRange = false;
        playerInRange = null;
    }


    void Update ()
    {
        timer += Time.deltaTime;

        if(timer >= timeBetweenAttacks && isPlayerInRange && enemyHealth.currentHealth > 0)
        {
            Attack ();
        }
    }


    void Attack ()
    {
        timer = 0f;
        if (!_playerHealths.Contains(playerInRange)) return;
        if(playerInRange.currentHealth > 0)
        {
            playerInRange.TakeDamage(attackDamage);
        }
    }
}
