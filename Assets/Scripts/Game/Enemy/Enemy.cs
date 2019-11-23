using UnityEngine;

public class Enemy 
{
    public int id;
    
    public EnemyMovement _enemyMovement;
    public EnemyHealth _enemyHealth;

    public EnemyState GetEnemyState()
    {
        var rigidBodyTransform = _enemyMovement.rigidbody.transform;
        var position = rigidBodyTransform.position;
        var rotation = rigidBodyTransform.rotation.eulerAngles;

        var health = _enemyHealth.currentHealth;
        return new EnemyState(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, health);
    }

    public override string ToString()
    {
        return GetEnemyState().ToString();
    }
    
}
