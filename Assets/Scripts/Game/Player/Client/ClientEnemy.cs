using UnityEngine;

public class ClientEnemy : MonoBehaviour
{
    private Animator _anim;
    private AudioSource _hurtAudio;
    
    public EnemyState state;
    private Rigidbody _enemyRigidBody;

    private int startingHealth = 100;
    
    private void Awake()
    {
        // ----------- Objects -----------
        _anim = GetComponent<Animator>();
        _hurtAudio = GetComponent<AudioSource>();
        _enemyRigidBody = GetComponent<Rigidbody>();
        // -----------        -----------

        // ----------- Position -----------
        var localTransform = transform;
        var position = localTransform.position;
        var rotation = localTransform.rotation;
        // Initialize the state
        state = new EnemyState( 
            position.x, position.y, position.z,
            rotation.x, rotation.y, rotation.z,
            startingHealth
        );
        // -----------        -----------
    }

    public ClientEnemy(EnemyState state)
    {
        this.state = state;
    } 
    
    private void UpdateMovement()
    {
        transform.position = new Vector3(state.x, state.y, state.z);
        _enemyRigidBody.MoveRotation(Quaternion.Euler(state.xA, state.yA, state.zA));
    }
    
    private void Update()
    {
        UpdateMovement();
    }
}