using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    Vector3 movement;                   // The vector to store the direction of the player's movement.
    Animator anim;                      // Reference to the animator component.
    Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
    int floorMask;                      // A layerdwd mask so that a ray can be cast just at gameobjects on the floor layer.
    float camRayLength = 100f;          // The length of the ray from the camera into the scene.

    private bool _isDead = false;
    
    
    void Awake ()
    {
        // Create a layer mask for the floor layer.
        floorMask = LayerMask.GetMask ("Floor");

        // Set up references.
        anim = GetComponent <Animator> ();
        playerRigidbody = GetComponent <Rigidbody> ();
    }

    private int counter = 0;
    public void Move (float h, float v)
    {
        if (_isDead) return;
        // Set the movement vector based on the axis input.
        movement.Set (h, 0f, v);

        // Normalise the movement vector and make it proportional to the speed per second.
        movement = movement.normalized * 0.2f;

        // Move the player to it's current position plus the movement.
        transform.position += movement;
    }

    public void Rotation(float xA, float yA, float zA)
    {
        if (_isDead) return;
        transform.rotation = Quaternion.Euler(xA, yA, zA);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetRotation()
    {
        return transform.rotation.eulerAngles;
    }

    public void Dead()
    {
        _isDead = true;
    }
}