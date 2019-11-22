public class Enemy
{
    private EnemyAttack _enemyAttack;
    private EnemyMovement _enemyMovement;
    private EnemyHealth _enemyHealth;


    public EnemyState GetEnemyState()
    {
        var position = _enemyMovement.transform.position;
        var health = _enemyHealth.currentHealth;
        
        return new EnemyState(health, position);
    }
}
