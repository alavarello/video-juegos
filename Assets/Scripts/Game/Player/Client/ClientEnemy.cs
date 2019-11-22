using UnityEngine;

public class ClientEnemy : MonoBehaviour
{
    public EnemyState state;

    public ClientEnemy(EnemyState state)
    {
        this.state = state;
    } 
}