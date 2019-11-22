using System;
using UnityEngine;
using UnityEngine.UI;

public class ClientPlayer : MonoBehaviour
{
    public static int playerId;
    public static Client client;

    // Scripts 
    public PlayerState state;

    // PlayerHealth
    public int startingHealth = 100;
    public int currentHealth;
    public AudioClip deathClip;

    // PlayerMovement
    private Vector3 _movement; // The vector to store the direction of the player's movement.

    private float _prevX, _prevZ;

    // Objects
    private Animator _anim;
    private AudioSource _hurtAudio;
    private Rigidbody _playerRigidBody;
    private PlayerShootingClient _playerShootingClient;

    public bool isDead;
    private bool _damaged;


    public void SetPlayerState(PlayerState playerState)
    {
        state = playerState;
    }

    private void Awake()
    {
        // ----------- Objects -----------
        _anim = GetComponent<Animator>();
        _hurtAudio = GetComponent<AudioSource>();
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
        _prevX = position.x;
        _prevZ = position.z;
        // -----------        -----------
    }

    public void UpdateHealth()
    {
        if (state.health == currentHealth) return;

        currentHealth = state.health;

        _hurtAudio.Play();

        if (currentHealth > 0 || isDead) return;

        isDead = true;
        _playerShootingClient.isDead = true;
        _anim.SetTrigger("Die");

        _hurtAudio.clip = deathClip;
        _hurtAudio.Play();

        if (this == client.cameraPlayer)
        {
            client.ChangeCamera();
        }
        
    }

    private void UpdateMovement()
    {
        transform.position = new Vector3(state.x, state.y, state.z);
        // Set the player's rotation to this new rotation.
        _playerRigidBody.MoveRotation(Quaternion.Euler(state.xA, state.yA, state.zA));
        // Animate the player.
        Animating();
    }

    public void Animating()
    {
        var walking = _prevX - state.x < 0.05 || _prevZ - state.z < 0.05;

        // Tell the animator whether or not the player is walking.
        _anim.SetBool("IsWalking", walking);
        
        _prevX = state.x;
        _prevZ = state.z;
    }

    // Update is called once per frame
    private void Update()
    {
        // Do not Update the Main Player do to prediction
        if (state.Id == playerId) return;
        UpdateHealth();
        UpdateMovement();
        if (state.isShooting)
        {
            _playerShootingClient.isShooting = true;
        }
    }

    private int counter = 0;

    // --------------- Prediction Methods ----------------------
    // These are exactly as the functions in PLayerMovement but in the client
    public void Move(float h, float v)
    {
        // Set the movement vector based on the axis input.
        _movement.Set(h, 0f, v);

        // Normalise the movement vector and make it proportional to the speed per second.
        _movement = _movement.normalized * 0.2f;

        // Move the player to it's current position plus the movement.
        transform.position += _movement;

    }

    public void Rotation(float xA, float yA, float zA)
    {
        transform.rotation = Quaternion.Euler(xA, yA, zA);
    }

    public void Shoot()
    {
        _playerShootingClient.isShooting = true;

    }

    public void UpdateState()
    {
        var position = transform.position;
        var rotation = transform.rotation;

        state = new PlayerState(playerId,
            position.x, position.y, position.z,
            rotation.x, rotation.y, rotation.z,
            currentHealth, _playerShootingClient.isShooting
        );
    }
}
