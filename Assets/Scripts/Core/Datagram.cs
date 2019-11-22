﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

public class Datagram
{
    public List<Message> messages;

    public Datagram(List<Message> messages)
    {
        this.messages = messages;
    }

    public Datagram(byte[] datagram , IPEndPoint from)
    {
        var ms = new MemoryStream();
        var bf = new BinaryFormatter();
        ms.Write(datagram, 0, datagram.Length);
        ms.Seek(0, SeekOrigin.Begin);
        messages = (List<Message>)bf.Deserialize(ms);
        foreach (var message in messages)
        {
            message.@from = from;
        }
    }

    public byte[] DatagramToByteArray()
    {
        var bf = new BinaryFormatter();
        var ms = new MemoryStream();
        bf.Serialize(ms, messages);
        return ms.ToArray();
    }
}

