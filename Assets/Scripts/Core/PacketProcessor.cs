﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class PacketProcessor
{
    private Connection _connection;

    private Unreliable _unreliable;

    private ReliableFast _reliableFast;
    private Dictionary<IPEndPoint, int> _reliableFastSequence;


    public PacketProcessor(string ip, int port, bool isListener)
    {
        _connection = new Connection(ip, port, isListener);
        _unreliable = new Unreliable();
        _reliableFast = new ReliableFast();
    }
    
    public List<Message> GetData() {
        // Get the lock
        var datagram =  _connection.GetData();
        if (datagram == null)
        {
            return null;
        }
        switch (datagram.messages[0].stream)
        {
            case Stream.Unreliable:
                return _unreliable.ProcessDatagram(datagram).messages;
            case Stream.ReliableFast:
                var processedDatagram = _reliableFast.ProcessDatagram(datagram, _connection);
                return processedDatagram?.messages;
            default:
                return null;
        }
        
    }

    public void SendUnreliableData(byte[] data, IPEndPoint ipEndPoint, MessageType messageType, int sequence) {
        // Send the data
        var message = new Message(messageType, Stream.Unreliable, _unreliable.GetMessageId(ipEndPoint), data, sequence);
        
        _unreliable.SendMessage(message, ipEndPoint, _connection);
    }
    
    public void SendReliableFastData(byte[] data, IPEndPoint ipEndPoint, MessageType messageType, int sequence) {
        // Send the data
        var message = new Message(messageType, Stream.ReliableFast,
            _reliableFast.GetMessageId(ipEndPoint), data, sequence);

        _reliableFast.SendMessage(message, ipEndPoint, _connection);
    }


}