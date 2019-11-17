using System;
using UnityEngine;

[Serializable]
public class Player : MonoBehaviour
{
    public int id;
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;
    public PlayerShooting playerShooting;
    public bool isShooting;

    private float _timer;
    private RaycastHit _shootHit;
    private int _shootabeMask;
    private ParticleSystem _gunParticles;
    private LineRenderer _gunLine;
    private AudioSource _gunAudio;
    private Light _gunLight;

    public int lastInputSequence;

    private void Awake()
    {
        playerShooting = GetComponentInChildren<PlayerShooting>();
    }


    public PlayerState GetPlayerState()
    {
        var position = playerMovement.GetPosition();
        var rotation = playerMovement.GetRotation();
        

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
}