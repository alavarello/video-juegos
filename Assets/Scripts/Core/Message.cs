﻿using System;
using System.Linq;
using System.Net;
[Serializable]
public class Message 
{
    public MessageType messageType;

    public Stream stream;

    public int messageId;
    
    public int sequence;
    
    public byte[] message;

    public IPEndPoint from;
    
    public Message(byte[] data, IPEndPoint fromIp)
    {
        stream = (Stream) BitConverter.ToInt32(data, 0);
        data = data.Skip(sizeof(Int32)).ToArray();
        messageId = BitConverter.ToInt32(data, 0);
        data = data.Skip(sizeof(Int32)).ToArray();
        messageType = (MessageType) BitConverter.ToInt32(data, 0);
        message = data.Skip(sizeof(Int32)).ToArray();
        from = fromIp;
    }

    public Message(MessageType messageType, Stream stream, int messageId, byte[] message, int sequence)
    {
        this.sequence = sequence;
        this.messageType = messageType;
        this.stream = stream;
        this.messageId = messageId;
        this.message = message;
    }

    public byte[] SerializeMessage()
    {
        var streamArray = BitConverter.GetBytes((Int32) stream);
        var messageIdArray = BitConverter.GetBytes((Int32) messageId);
        var messageTypeArray = BitConverter.GetBytes((Int32) messageType);
        return streamArray.Concat(messageIdArray).Concat(messageTypeArray).Concat(this.message).ToArray();
    }
}