using System.Net.Mail;
using System.Numerics;
using UnityEngine;

public class EnemyState
{
    public int health;
    public int id;
    public float x, y, z;
    public float xA, yA, zA;
    // Hit particles
    public float xH, yH, zH;

    public EnemyState(int id, float x, float y, float z, float xA, float yA, float zA, int health)
    {
        this.id = id;
        this.health = health;
        this.x = x;
        this.y = y;
        this.z = z;
        this.xA = xA;
        this.yA = yA;
        this.zA = zA;
    }
    
    public EnemyState(float x, float y, float z, float xA, float yA, float zA, int health)
    {
        this.health = health;
        this.x = x;
        this.y = y;
        this.z = z;
        this.xA = xA;
        this.yA = yA;
        this.zA = zA;
    }
    
    public void AddHitPoint(float xH, float yH, float zH)
    {
        this.xH = x;
        this.yH = y;
        this.zH = z;
        
    }

    public EnemyState(BitBuffer bitBuffer)
    {
        id = bitBuffer.GetInt(0, 100);

        x = bitBuffer.GetFloat(-100, 100, (float)0.1);
        z = bitBuffer.GetFloat(-100, 100, (float)0.1);
        
        yA = bitBuffer.GetInt(0, 360);
        
        xH = bitBuffer.GetFloat(-100, 100, (float)0.1);
        yH = bitBuffer.GetFloat(-100, 100, (float)0.1);
        zH = bitBuffer.GetFloat(-100, 100, (float)0.1);

        health = bitBuffer.GetInt(0, 100);
    }

    public void Serialize(BitBuffer bitBuffer)
    {
        bitBuffer.PutInt(id, 0, 100);

        bitBuffer.PutFloat(x, -100, 100, (float)0.1);
        bitBuffer.PutFloat(z, -100, 100, (float)0.1);
        
        bitBuffer.PutInt((int) yA, 0, 360);
        
        bitBuffer.PutFloat(xH, -100, 100, (float)0.1);
        bitBuffer.PutFloat(yH, -100, 100, (float)0.1);
        bitBuffer.PutFloat(zH, -100, 100, (float)0.1);
        
        bitBuffer.PutInt(health, 0, 100);
    }

    public override string ToString()
    {
        return id + " " + x + " " + z + " " + yA + " " + health;
    }
}
