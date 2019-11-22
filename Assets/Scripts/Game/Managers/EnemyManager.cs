using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemy;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;
    public static int enemyIdCounter;

    
    // TODO ADD game over
    private bool gameOver = false; 
    void Start ()
    {
        InvokeRepeating ("Spawn", spawnTime, spawnTime);
    }


    void Spawn ()
    {
        if(gameOver)
        {
            return;
        }

        var spawnPointIndex = Random.Range(0, spawnPoints.Length);
        var position = spawnPoints[spawnPointIndex].position;
        var rotations = spawnPoints[spawnPointIndex].rotation;

        Server.enemies[enemyIdCounter++] = new Enemy();

        Instantiate (enemy, position, rotations);
    }
}
