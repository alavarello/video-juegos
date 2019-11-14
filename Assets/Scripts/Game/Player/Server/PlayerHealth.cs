using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public int currentHealth;

    PlayerMovement playerMovement;
    bool isDead;
    bool damaged;


    void Awake ()
    {
        playerMovement = GetComponent <PlayerMovement> ();
        currentHealth = startingHealth;
    }

    public void TakeDamage (int amount)
    {
        damaged = true;

        currentHealth -= amount;

        if(currentHealth <= 0 && !isDead)
        {
            Death ();
        }
    }


    void Death ()
    {
        isDead = true;

        playerMovement.enabled = false;
    }


    public void RestartLevel ()
    {
        SceneManager.LoadScene (0);
    }
}
