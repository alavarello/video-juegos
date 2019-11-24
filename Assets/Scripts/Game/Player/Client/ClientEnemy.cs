using UnityEngine;

public class ClientEnemy : MonoBehaviour
{
    private Animator _anim;
    public AudioClip deathClip;
    
    public EnemyState state;
    private Rigidbody _enemyRigidBody;
    public float sinkSpeed = 2.5f;

    ParticleSystem hitParticles;

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
        hitParticles = GetComponentInChildren <ParticleSystem> ();

    }
    
    public void TakeDamage (Vector3 hitPoint)
    {
        if(isDead)
            return;
        
        enemyAudio.Play();
        hitParticles.transform.position = hitPoint;
        hitParticles.Play();
        
        if(state.health <= 20)
        {
            isDead = true;

            capsuleCollider.isTrigger = true;

            _anim.SetTrigger("Dead");

            enemyAudio.clip = deathClip;
            enemyAudio.Play ();
            StartSinking();
        }
    }

    public ClientEnemy(EnemyState state)
    {
        this.state = state;
    } 
    
    private void UpdateMovement()
    {
        transform.position = new Vector3(state.x, state.y, state.z);
        transform.rotation = Quaternion.Euler(state.xA, state.yA, state.zA);
    }
    
    private void Update()
    {
        if(isSinking)
        {
            transform.Translate (Time.deltaTime * sinkSpeed * -Vector3.up);
        }
        else
        {
            UpdateMovement();
        }
    }
    
    public void StartSinking ()
    {
        GetComponent <UnityEngine.AI.NavMeshAgent> ().enabled = false;
        GetComponent <Rigidbody> ().isKinematic = true;
        isSinking = true;
        Destroy (gameObject, 2f);
    }
}