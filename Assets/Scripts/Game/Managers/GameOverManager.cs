using UnityEngine;

public class GameOverManager : MonoBehaviour
{

    public static Client client;
    
    Animator anim;                          // Reference to the animator component.
    float restartTimer;                     // Timer to count up to restarting the level


    void Awake ()
    {
        // Set up the reference.
        anim = GetComponent <Animator> ();
    }


    void Update ()
    {
        foreach (var player in client.players.Values)
        {
            if (!player.isDead) return;
        }

 
        // ... tell the animator the game is over.
        anim.SetTrigger ("GameOver");

    }
}