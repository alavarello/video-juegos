using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    Transform player;
    EnemyHealth enemyHealth;
    UnityEngine.AI.NavMeshAgent nav;


    void Awake ()
    {
        enemyHealth = GetComponent <EnemyHealth> ();
        nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
    }


    void Update ()
    {

        if (player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag ("Player");
            player = playerObject.transform;
            return;
        }
        if(enemyHealth.currentHealth > 0 /*&& playerHealth.currentHealth > 0*/)
        {
            nav.SetDestination (player.position);
        }
        else
        {
            nav.enabled = false;
        }
    }
}
