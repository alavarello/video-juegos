public class Enemy
{
    public int id;
    
    private EnemyAttack _enemyAttack;
    private EnemyMovement _enemyMovement;
    private EnemyHealth _enemyHealth;


    public EnemyState GetEnemyState()
    {
        var position = _enemyMovement.transform.position;
        var rotation = _enemyMovement.transform.rotation;
        var health = _enemyHealth.currentHealth;
        
        return new EnemyState(id, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, health);
    }
}
