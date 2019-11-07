﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class Buffer
{
    private Dictionary<int, Message> _messages = new Dictionary<int, Message>();
    private int _removeId = 0;
    public IPEndPoint id;
    
    public Buffer(IPEndPoint id)
    {
        this.id = id;
    }

    public void RemoveMessages(int lastAck)
    {
        List<int> acks = new List<int>();
        foreach (var messageId in _messages.Keys)
        {
            if (messageId <= lastAck)
            {
                acks.Add(messageId);
            }
        }

        foreach (var ack in acks)
        {
            _messages.Remove(ack);
        }
    }

    public void AddMessage(Message message)
    {
        if (!_messages.ContainsKey(message.messageId))
        {
            _messages.Add(message.messageId, message);
        }
        while (_messages.Count > 2)
        {
            _messages.Remove(_removeId++);
        }
    }

    public Datagram BufferDatagram()
    {
        return new Datagram(new List<Message>(_messages.Values));
    }

}