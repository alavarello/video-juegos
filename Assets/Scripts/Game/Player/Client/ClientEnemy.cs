using UnityEngine;

public class ClientEnemy : MonoBehaviour
{
    private Animator _anim;
    public AudioClip deathClip;
    
    public EnemyState state;
    private Rigidbody _enemyRigidBody;

    private int startingHealth = 100;
    
    private AudioSource enemyAudio;
    private CapsuleCollider capsuleCollider;
    bool isDead;
    bool isSinking;

    private void Awake()
    {
        // ----------- Objects -----------
        _anim = GetComponent<Animator>();
        deathClip = GetComponent<AudioClip>();
        _enemyRigidBody = GetComponent<Rigidbody>();
        enemyAudio = GetComponent <AudioSource> ();
        capsuleCollider = GetComponent <CapsuleCollider> ();

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
    
    public void TakeDamage ()
    {
        if(isDead)
            return;

        enemyAudio.Play();

        if(state.health <= 0)
        {
            isDead = true;

            capsuleCollider.isTrigger = true;

            _anim.SetTrigger("Dead");

            enemyAudio.clip = deathClip;
            enemyAudio.Play ();
            Client.enemies.Remove(state.id);
            Destroy (gameObject, 2f);
        }
    }
    
    public void StartSinking ()
    {
        GetComponent <UnityEngine.AI.NavMeshAgent> ().enabled = false;
        GetComponent <Rigidbody> ().isKinematic = true;
        isSinking = true;
        Destroy (gameObject, 2f);
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