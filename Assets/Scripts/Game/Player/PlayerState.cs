using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class PlayerState
{
    private int id;
    public float x, y, z;
    public float xA, yA, zA;
    public int health;
    public bool isShooting;

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

    public PlayerState()
    {
    }

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public byte[] Serialize()
    {
        var bf = new BinaryFormatter();
        var ms = new MemoryStream();
        bf.Serialize(ms, this);
        return ms.ToArray();
    }

    public PlayerState deserialize(byte[] bytes)
    {
        var ms = new MemoryStream();
        var bf = new BinaryFormatter();
        ms.Write(bytes, 0, bytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        return (PlayerState)bf.Deserialize(ms);
    }
}