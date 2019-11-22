using UnityEngine;

public class EnemyState
{
    public int health;
    public int id;
    public float x, y, z;
    public float xA, yA, zA;

    public EnemyState(int health, Vector3 position)
    {
        this.health = health;
        x = position.x;
        y = position.y;
        z = position.z;
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

    public EnemyState(BitBuffer bitBuffer)
    {
        id = bitBuffer.GetInt(0, 10);

        x = bitBuffer.GetFloat(-100, 100, (float)0.1);
        z = bitBuffer.GetFloat(-100, 100, (float)0.1);
        
        xA = bitBuffer.GetInt(0, 360);
        yA = bitBuffer.GetInt(0, 360);
        zA = bitBuffer.GetInt(0, 360);

        health = bitBuffer.GetInt(0, 100);
    }

    public void Serialize(BitBuffer bitBuffer)
    {
        bitBuffer.PutInt(id, 0, 100);

        bitBuffer.PutFloat(x, -100, 100, (float)0.1);
        bitBuffer.PutFloat(z, -100, 100, (float)0.1);
        
        bitBuffer.PutInt((int) xA, 0, 360);
        bitBuffer.PutInt((int) yA, 0, 360);
        bitBuffer.PutInt((int) zA, 0, 360);

        bitBuffer.PutInt(health, 0, 100);
    }
}
