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

    public PlayerState (BitBuffer bitBuffer)
    {
        id = bitBuffer.GetInt(0, 10);
        
        x = bitBuffer.GetFloat(-100, 100, (float)0.1);
        z = bitBuffer.GetFloat(-100, 100, (float)0.1);

        xA = bitBuffer.GetInt(0, 360);
        yA = bitBuffer.GetInt(0, 360);
        zA = bitBuffer.GetInt(0, 360);
        
        health = bitBuffer.GetInt(0, 100);
        isShooting = bitBuffer.GetBit();
        
        sequence = bitBuffer.GetInt(0, 10000);
    }
    
    public void serialize(BitBuffer bitBuffer)
    {
        bitBuffer.PutInt(id, 0, 10);
       
        bitBuffer.PutFloat(x, -100, 100, (float)0.1);
        bitBuffer.PutFloat(z, -100, 100, (float)0.1);

        bitBuffer.PutInt((int) xA, 0, 360);
        bitBuffer.PutInt((int) yA, 0, 360);
        bitBuffer.PutInt((int) zA, 0, 360);
        
        bitBuffer.PutInt(health, 0, 100);
        bitBuffer.PutBit(isShooting);
        
        bitBuffer.PutInt(sequence, 0, 10000);
    }
}