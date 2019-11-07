using System;
using UnityEngine;
[Serializable]
public class Player: MonoBehaviour
{
 public int id;
 public Transform transform;
 public PlayerHealth playerHealth;
 public PlayerMovement playerMovement;
 public int count = 0;
 
 public int damagePerShot = 20;
 public float timeBetweenBullets = 0.15f;
 public float range = 100f;
 public bool isShooting = false; 

 float timer;
 Ray shootRay = new Ray();
 RaycastHit shootHit;
 int shootableMask;
 ParticleSystem gunParticles;
 LineRenderer gunLine;
 AudioSource gunAudio;
 Light gunLight;
 float effectsDisplayTime = 0.2f;
 

 public Player()
 {
     shootableMask = LayerMask.GetMask ("Shootable");
 }

 public PlayerState GetPlayerState()
 {
     var position = playerMovement.GetPosition();
     var rotation = playerMovement.GetRotation();
     
//     if (count % 60 == 1)
//     {
//         playerHealth.currentHealth = playerHealth.currentHealth - 10;
//     }
//
//     count++;

     var state = new PlayerState(
         position.x, position.y, position.z,
         rotation.x, rotation.y, rotation.z, 
         playerHealth.currentHealth, isShooting
         );
     // The shooting only is for one snapshot
     isShooting = false;
     state.Id = id;
     return state;
 }
 
 public void Shoot ()
 {
     isShooting = true;
     shootRay.origin = transform.position;
     shootRay.direction = transform.forward;

     if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
     {
         EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth> ();
         if(enemyHealth != null)
         {
             enemyHealth.TakeDamage (damagePerShot, shootHit.point);
         }
     }
 }
}