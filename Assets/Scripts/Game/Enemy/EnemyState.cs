using System.Net.Mail;
using System.Numerics;
using UnityEngine;

public class EnemyState
{
    public EnemyType type;
    public int health;
    public int id;
    public float x, y, z;
    public float xA, yA, zA;
    // Hit particles
    public float xH, yH, zH;
    

    public EnemyState(int id, float x, float y, float z, float xA, float yA, float zA, int health, EnemyType type)
    {
        this.id = id;
        this.health = health;
        this.x = x;
        this.y = y;
        this.z = z;
        this.xA = xA;
        this.yA = yA;
        this.zA = zA;
        this.type = type;
    }
    
    public EnemyState(float x, float y, float z, float xA, float yA, float zA, int health, EnemyType type)
    {
        this.health = health;
        this.x = x;
        this.y = y;
        this.z = z;
        this.xA = xA;
        this.yA = yA;
        this.zA = zA;
        this.type = type;
    }
    
    public void AddHitPoint(float xH, float yH, float zH)
    {
        this.xH = xH;
        this.yH = yH;
        this.zH = zH;
        
    }

    public EnemyState(BitBuffer bitBuffer)
    {
        id = bitBuffer.GetInt(0, 1000);

        type = (EnemyType)bitBuffer.GetInt(0, 3);

        x = bitBuffer.GetFloat(-200, 200, (float)0.1);
        z = bitBuffer.GetFloat(-200, 200, (float)0.1);
        
        yA = bitBuffer.GetInt(0, 360);

        xH = bitBuffer.GetFloat(-200, 200, (float)0.1);
        yH = bitBuffer.GetFloat(-200, 200, (float)0.1);
        zH = bitBuffer.GetFloat(-200, 200, (float)0.1);

        health = bitBuffer.GetInt(0, 200);
    }

    public void Serialize(BitBuffer bitBuffer)
    {
        bitBuffer.InsertInt(id, 0, 1000);
        
        bitBuffer.InsertInt((int)type, 0, 3);
        bitBuffer.PutFloat(x, -200, 200, (float)0.1);
        bitBuffer.PutFloat(z, -200, 200, (float)0.1);
        
        bitBuffer.InsertInt((int) yA, 0, 360);
        
        bitBuffer.PutFloat(xH, -200, 200, (float)0.1);
        bitBuffer.PutFloat(yH, -200, 200, (float)0.1);
        bitBuffer.PutFloat(zH, -200, 200, (float)0.1);
        
        bitBuffer.InsertInt(health, 0, 200);
    }

    public override string ToString()
    {
        return id + " " + x + " " + z + " " + yA + " " + health;
    }
}
