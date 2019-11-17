using System;

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

    public PlayerState()
    {
    }

    public int Id
    {
        get => id;
        set => id = value;
    }

    public bool isInTheSamePosition(PlayerState other)
    {
        if (Math.Abs(other.x - x) > 0.01 || Math.Abs(other.z - z) > 0.01) return false;
        return true;
    }
    
    public bool isInTheSameRotation(PlayerState other)
    {
        if (Math.Abs(other.xA - xA) > 0.1 || Math.Abs(other.zA - zA) > 0.1 || Math.Abs(other.yA - yA) > 0.1) return false;
        return true;
    }
    
}