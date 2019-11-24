﻿using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemy;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;
    public static int enemyIdCounter;

    public int maxForLevel = 20;
    // TODO ADD game over
    private bool gameOver = false; 
    void Start ()
    {
        InvokeRepeating ("Spawn", spawnTime, spawnTime);
    }


    void Spawn ()
    {
        if(gameOver || enemyIdCounter > 3)
        {
            return;
        }

        var spawnPointIndex = Random.Range(0, spawnPoints.Length);
        var position = spawnPoints[spawnPointIndex].position;
        var rotations = spawnPoints[spawnPointIndex].rotation;

        var newEnemy = Instantiate (enemy, position, rotations);
        
        var enemyScript = new Enemy();

        enemyScript.id = enemyIdCounter;
        enemyScript._enemyMovement = newEnemy.GetComponent<EnemyMovement>();
        enemyScript._enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        enemyScript._enemyHealth.id = enemyIdCounter;

        Server.enemies[enemyIdCounter] = enemyScript;

        enemyIdCounter++;
    }
}
