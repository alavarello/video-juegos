﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Server
{
    private readonly List<IPEndPoint> _ipEndPoints = new List<IPEndPoint>();

    private readonly PacketProcessor _packetProcessor;

    private readonly Dictionary<int, Player> _players;
    
    private readonly Engine _engine;

    private int _sequence = 0;
    
    private int _updateCounter = 0;
    
    public static int score = 0;

    
    // Start is called before the first frame update
    public Server(Engine engine)
    {
        _engine = engine;
        foreach (var ip in engine.IPs)
        {
            _ipEndPoints.Add(new IPEndPoint(IPAddress.Parse(ip), engine.clientListeningPort));
        }
        
        _packetProcessor = new PacketProcessor(null, engine.serverListeningPort, true);
        
        var idCounter = 0;
        _players = new Dictionary<int, Player>();
        
        foreach (var ips in engine.IPs)
        {
            var id = idCounter++;
            var playerScript = CreatePlayer();
            playerScript.id = id;
            playerScript.playerHealth = playerScript.GetComponent<PlayerHealth>();
            playerScript.playerMovement = playerScript.GetComponent<PlayerMovement>();
            _players.Add(id, playerScript);
        }
        
    }

    private static Player CreatePlayer()
    {
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Server/Player");
        var player = GameObject.Instantiate<GameObject>(playerPrefab);  
        var playerScript = player.GetComponent<Player>();
        return playerScript;
    }

    // Update is called once per frame
    public void Update()
    {
        _updateCounter++;
        if (_updateCounter % (60/_engine.serverSps) != 0) return;

        var data = _packetProcessor.GetData();
         while (data != null)
         {
            foreach (var message in data)
            {
                var id = BitConverter.ToInt32(message.message, 0);
                if (message.messageType == MessageType.Input)
                {
                    
                    var h = BitConverter.ToInt32(message.message, sizeof(Int32));
                    var v = BitConverter.ToInt32(message.message, sizeof(Int32)*2);
                    _players[id].playerMovement.Move(h, v);
                } else if (message.messageType == MessageType.Rotation)
                {
                    var x = BitConverter.ToInt32(message.message, sizeof(Int32));
                    var y = BitConverter.ToInt32(message.message, sizeof(Int32)*2);
                    var z = BitConverter.ToInt32(message.message, sizeof(Int32)*3);
                    _players[id].playerMovement.Rotation(x, y, z);
                } else if (message.messageType == MessageType.Fire)
                {
                   _players[id].playerShooting.Shoot();
                   _players[id].isShooting = true;
                }

                if (_players[id].lastInputSequence < message.sequence)
                {
                    _players[id].lastInputSequence = message.sequence;
                }

            }
            data = _packetProcessor.GetData();
         }

        var playerStates = _players.Values.Select(player => player.GetPlayerState()).ToList();

        var bf = new BinaryFormatter();
        var ms = new MemoryStream();
        bf.Serialize(ms, playerStates);

        foreach (var ipEndPoint in _ipEndPoints)
        {
            _packetProcessor.SendUnreliableData(
                BitConverter.GetBytes(score).Concat(ms.ToArray()).ToArray(), ipEndPoint, MessageType.Input, _sequence);
        }
        _sequence++;
    }
}
