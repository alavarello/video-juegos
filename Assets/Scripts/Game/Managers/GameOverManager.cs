using UnityEngine;

public class GameOverManager : MonoBehaviour
{

    public static Client client;
    
    Animator anim;                          // Reference to the animator component.


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

        if (client.players.Count == 0) return;
 
        // ... tell the animator the game is over.
        anim.SetTrigger ("GameOver");
        Debug.Log("LLEGUEEEEEEEEEEEEE");

    }
}