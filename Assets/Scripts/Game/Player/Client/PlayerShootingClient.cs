using System;
using UnityEngine;

public class PlayerShootingClient : MonoBehaviour
{

    public float range = 100f;
    private float timer = 0;                                    // A timer to determine when to fire.
    public float timeBetweenBullets = 0.15f;        // The time between each shot.

    Ray shootRay = new Ray();
    RaycastHit shootHit;
    ParticleSystem gunParticles;
    LineRenderer gunLine;
    AudioSource gunAudio;
    Light gunLight;
    float effectsDisplayTime = 0.2f;
    public bool isShooting;
    public bool isDead = false;
    void Awake ()
    {
        gunParticles = GetComponent<ParticleSystem> ();
        gunLine = GetComponent <LineRenderer> ();
        gunAudio = GetComponent<AudioSource> ();
        gunLight = GetComponent<Light> ();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (isShooting && !isDead)
        {
            Shoot();
            isShooting = false;
            timer = 0;
        } 
        if(timer >= timeBetweenBullets * effectsDisplayTime)
        {
            isShooting = false;
            DisableEffects();
        }
    }


    public void DisableEffects ()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }

    public void Shoot ()
    {

        gunAudio.Play ();

        gunLight.enabled = true;

        gunParticles.Stop ();
        gunParticles.Play ();

        gunLine.enabled = true;
        gunLine.SetPosition (0, transform.position);

        shootRay.origin = transform.position;
        shootRay.direction = transform.forward;
        gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
    }

}
