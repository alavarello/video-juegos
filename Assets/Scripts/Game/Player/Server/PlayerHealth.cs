using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerHealth : MonoBehaviour
{

    public static Engine engine;
    
    public int startingHealth = 100;
    public int currentHealth;

    private PlayerMovement _playerMovement;
    private PlayerShooting _playerShooting;
    private bool _isDead;
    private bool _damaged;


    void Awake ()
    {
        _playerMovement = GetComponent <PlayerMovement> ();
        _playerShooting = GetComponentInChildren<PlayerShooting>();
        currentHealth = startingHealth;
    }

    public void TakeDamage (int amount)
    {
        _damaged = true;

        currentHealth -= amount;

        if(currentHealth <= 0 && !_isDead)
        {
            Death ();
        }
    }


    void Death ()
    {
        _isDead = true;

        _playerMovement.Dead();
        _playerShooting.Dead();
        
        engine.server.playerDied(this);
        
    }
}
