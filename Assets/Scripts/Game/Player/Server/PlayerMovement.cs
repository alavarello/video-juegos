using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;            // The speed that the player will move at.

    Vector3 movement;                   // The vector to store the direction of the player's movement.
    Animator anim;                      // Reference to the animator component.
    Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
    int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    float camRayLength = 100f;          // The length of the ray from the camera into the scene.

    void Awake ()
    {
        // Create a layer mask for the floor layer.
        floorMask = LayerMask.GetMask ("Floor");

        // Set up references.
        anim = GetComponent <Animator> ();
        playerRigidbody = GetComponent <Rigidbody> ();
    }

    public void Move (float h, float v)
    {
        // Set the movement vector based on the axis input.
        movement.Set (h, 0f, v);

        // Normalise the movement vector and make it proportional to the speed per second.
        movement = movement.normalized * (speed * Time.deltaTime * 3);

        // Move the player to it's current position plus the movement.
        playerRigidbody.MovePosition (transform.position + movement);
    }

    public void Rotation(float xA, float yA, float zA)
    {
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
}