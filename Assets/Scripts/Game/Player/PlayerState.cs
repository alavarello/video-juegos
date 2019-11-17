using System;
using UnityEngine;

[Serializable]
public class PlayerState
{
    private int id;
    public float x, y, z;
    public float xA, yA, zA;
    public int health;
    public bool isShooting;
    public int sequence;

    public PlayerState(float x, float y, float z, float xA, float yA, float zA, int health, bool isShooting)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.xA = xA;
        this.yA = yA;
        this.zA = zA;
        this.health = health;
        this.isShooting = isShooting;
    }
    
    public PlayerState(float x, float y, float z, float xA, float yA, float zA, int health, bool isShooting, int sequence)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.xA = xA;
        this.yA = yA;
        this.zA = zA;
        this.health = health;
        this.isShooting = isShooting;
        this.sequence = sequence;
    }

    public int Id
    {
        get => id;
        set => id = value;
    }

    public bool IsInTheSamePosition(PlayerState other)
    {   
        if (Math.Abs(other.x - x) > 0.1 || Math.Abs(other.z - z) > 0.1)
        {
            Debug.Log("Client: " + x + " " + z + " " + sequence + " Snapshot: " + other.x + " " + other.z + " " + sequence);
            return false;
        }
        return true;
    }
    
    public bool IsInTheSameRotation(PlayerState other)
    {
        if (Math.Abs(other.xA - xA) > 0.1 || Math.Abs(other.zA - zA) > 0.1 || Math.Abs(other.yA - yA) > 0.1) return false;
        return true;
    }

    public PlayerState Clone()
    {
        return new PlayerState(x, y, z, xA, yA, zA, health, isShooting, sequence);
    }
    
}