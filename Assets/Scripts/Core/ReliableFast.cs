﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class ReliableFast
{
    
    Dictionary<IPEndPoint, Buffer> buffers = new Dictionary<IPEndPoint, Buffer>();
    Dictionary<IPEndPoint, int> _acks = new Dictionary<IPEndPoint, int>();
    private Dictionary<IPEndPoint, int> _ids = new Dictionary<IPEndPoint, int>();

    public int GetMessageId(IPEndPoint ipEndPoint)
    {
        if (!_ids.ContainsKey(ipEndPoint))
            _ids.Add(ipEndPoint, 0);

        var sequenceValue = _ids[ipEndPoint];
        sequenceValue++;
        _ids[ipEndPoint] = sequenceValue;
        return sequenceValue;
    }

    public Datagram ProcessDatagram(Datagram datagram, Connection connection)
    {
        if (datagram == null)
        {
            return null;
        }

        var from = datagram.messages[0].from;
        if (!buffers.ContainsKey(from))
        {
            buffers.Add(from, new Buffer(from));
        }

        if (datagram.messages[0].messageType == MessageType.Ack)
        {
            var message = datagram.messages[0];
            var ack = BitConverter.ToInt32(message.message, 0);
            buffers[message.from].RemoveMessages(ack);
            return null;
        }

        var maxId = 0;
        foreach (var message in datagram.messages)
        {
            buffers[message.from].AddMessage(message);
            if (message.messageId >= maxId)
            {
                maxId = message.messageId;
            }
        }
        
        SendACK(maxId, datagram.messages[0].from, connection);
        
        return datagram;
    }


    public void SendACK(Int32 ack, IPEndPoint ipEndPoint, Connection connection)
    {
        var data = BitConverter.GetBytes(ack);
        // No need to send the sequence with the ack
        var message = new Message(MessageType.Ack, Stream.ReliableFast, GetMessageId(ipEndPoint), data, 0);
        var messages = new List<Message>();
        messages.Add(message);
        var datagram = new Datagram(messages);
        connection.SendData(datagram.DatagramToByteArray(), ipEndPoint);

    }

    public void SendMessage(Message message, IPEndPoint ipEndPoint, Connection connection) {
        // Send the data
        if (!buffers.ContainsKey(ipEndPoint))
        {
            buffers.Add(ipEndPoint, new Buffer(ipEndPoint));
        }
        
        buffers[ipEndPoint].AddMessage(message);

        connection.SendData(buffers[ipEndPoint].BufferDatagram().DatagramToByteArray(), ipEndPoint);
    }
}