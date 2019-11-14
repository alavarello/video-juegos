using System;
using UnityEngine;
using UnityEngine.UI;

public class ClientPlayer: MonoBehaviour
{
    // Scripts 
     private PlayerState state;
     
     // PlayerHealth
     public int startingHealth = 100;
     public int currentHealth;
     public AudioClip deathClip;

     // PlayerMovement
     public float speed = 6f;            // The speed that the player will move at.
     private Vector3 _movement;                   // The vector to store the direction of the player's movement.

     private float prevX, prevZ;
     // Objects
     private Animator _anim;
     private AudioSource _hurtAudio;
     private Rigidbody _playerRigidBody;
     private PlayerShootingClient _playerShootingClient;

     bool isDead;
     bool damaged;
     

    public PlayerState getPlayerState()
    {
        return state;
    }

    public byte[] Serialize()
    {
        return state.Serialize();
    }

    public void UpdateState(byte[] bytes)
    {
        state = state.deserialize(bytes);
    }

    public void SetId(int id)
    {
        state.Id = id;
    }
    
    public void SetPlayerState(PlayerState playerState)
    {
        state = playerState;
    }

    private void Awake()
    {
        // ----------- Objects -----------
        _anim = GetComponent <Animator> ();
        _hurtAudio = GetComponent <AudioSource> ();
        _playerRigidBody = GetComponent<Rigidbody>();
        _playerShootingClient = GetComponentInChildren<PlayerShootingClient>();
        // -----------        -----------
        
        // ----------- Health -----------
        currentHealth = startingHealth;
        // -----------        -----------

        // ----------- Position -----------
        var localTransform = transform;
        var position = localTransform.position;
        var rotation = localTransform.rotation;
        // Initialize the state
        state = new PlayerState(
            position.x, position.y, position.z, 
            rotation.x, rotation.y, rotation.z, 
            startingHealth, false
            );
        prevX = position.x;
        prevZ = position.z;
        // -----------        -----------
    }

    private void UpdateHealth()
    {
        if(state.health != currentHealth)
        {
            currentHealth = state.health;

            _hurtAudio.Play ();

            if(currentHealth <= 0 && !isDead)
            {
                isDead = true;
                _playerShootingClient.isDead = true;
                _anim.SetTrigger ("Die");

                _hurtAudio.clip = deathClip;
                _hurtAudio.Play ();

            }
        }
    }

    void UpdateMovement ()
    {
        transform.position = new Vector3(state.x+2, state.y, state.z+2);
        // Set the player's rotation to this new rotation.
        _playerRigidBody.MoveRotation (Quaternion.Euler(state.xA, state.yA, state.zA));
        // Animate the player.
        Animating ();

        prevX = state.x;
        prevZ = state.z;
    }

    void Animating ()
    {
        bool walking = prevX - state.x < 0.05 || prevZ - state.z < 0.05;

        // Tell the animator whether or not the player is walking.
        _anim.SetBool ("IsWalking", walking);
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        if (state.isShooting)
        {
            _playerShootingClient.isShooting = true;
        }

    }

    // Update is called once per frame
    private void Update()
    {
     UpdateHealth();
    }
}
