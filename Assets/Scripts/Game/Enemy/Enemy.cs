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
        var enemyState =  new EnemyState(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, health);
        var hitPoint = _enemyHealth.hitPoint;
        enemyState.AddHitPoint(hitPoint.x, hitPoint.y, hitPoint.z);
        return enemyState;
    }

    public override string ToString()
    {
        return GetEnemyState().ToString();
    }
    
}
