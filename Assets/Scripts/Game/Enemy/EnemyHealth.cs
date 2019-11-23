using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int id;
    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public int scoreValue = 10;
    public AudioClip deathClip;


    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    public bool isDead;


    void Awake ()
    {
        anim = GetComponent <Animator> ();
        enemyAudio = GetComponent <AudioSource> ();
        hitParticles = GetComponentInChildren <ParticleSystem> ();
        capsuleCollider = GetComponent <CapsuleCollider> ();

        currentHealth = startingHealth;
    }


    public void TakeDamage (int amount, Vector3 hitPoint)
    {
        if(isDead)
            return;

        currentHealth -= amount;
        hitParticles.transform.position = hitPoint;
        hitParticles.Play();
        if(currentHealth <= 0)
        {
            isDead = true;
        }
    }

    public void DestroyGameObjectDestroy()
    {
        Destroy(gameObject, 1f);
    }
}
