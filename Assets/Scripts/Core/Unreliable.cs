﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class Unreliable
{
    private Dictionary<IPEndPoint, int> _id = new Dictionary<IPEndPoint, int>();

    public int GetMessageId(IPEndPoint ipEndPoint)
    {
        if (!_id.ContainsKey(ipEndPoint))
            _id.Add(ipEndPoint, 0);

        var sequenceValue = _id[ipEndPoint];
        sequenceValue++;
        _id[ipEndPoint] = sequenceValue;
        return sequenceValue;
    }

    public Datagram ProcessDatagram(Datagram datagram)
    {
        return datagram;
    }
    
    public void SendMessage(Message message, IPEndPoint ipEndPoint, Connection connection) {
        // Send the data
        var messages = new List<Message>();
        messages.Add(message);
        var datagram = new Datagram(messages);
        connection.SendData(datagram.DatagramToByteArray(), ipEndPoint);
    }
}