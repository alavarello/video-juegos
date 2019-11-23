using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{

    public static Engine engine;
    
    List<Transform> players;
    EnemyHealth enemyHealth;
    UnityEngine.AI.NavMeshAgent nav;
    public Rigidbody rigidbody;

    void Awake ()
    {
        enemyHealth = GetComponent <EnemyHealth> ();
        nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
        rigidbody = GetComponent<Rigidbody>();
        players = engine.server.playersTransforms;
    }
    
    void Update ()
    {
        if (players.Count == 0) return;

        if(enemyHealth.currentHealth > 0)
        {
            var target = GetNearestTarget();
            nav.SetDestination (target.position);
        }
        else
        {
            nav.enabled = false;
        }
    }
    
   

    private Transform GetNearestTarget()
    {
        var minDistance = float.MaxValue;
        Transform minTransform = null;
        
        foreach (var playerTransform in players)
        {
            var distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                minTransform = playerTransform;
            }
        }

        return minTransform;
    }
}
