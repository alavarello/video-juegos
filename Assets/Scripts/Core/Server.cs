using System;
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

    private int _lastMessageReceived = -1;

    private readonly Engine _engine;

    private int _sequence = 0;

    private float _timeForNextSnapshot = 0;

    public static int score = 0;

    public readonly List<Transform> playersTransforms = new List<Transform>();
    public readonly List<PlayerHealth> playersHealth = new List<PlayerHealth>();
    public readonly List<GameObject> playersObjects = new List<GameObject>();

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
            playersTransforms.Add(playerScript.playerMovement.transform);
            playersHealth.Add(playerScript.playerHealth);
            playersObjects.Add(playerScript.gameObject);
        }
        
    }

    private static Player CreatePlayer()
    {
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Server/Player");
        var player = GameObject.Instantiate<GameObject>(playerPrefab);  
        var playerScript = player.GetComponent<Player>();
        return playerScript;
    }

    private void readMessage(Message message)
    {
        // Prevent ReliableFast double input problem
        if (_lastMessageReceived >= message.messageId) return;

        _lastMessageReceived = message.messageId;
                
        BitBuffer bitBuffer = new BitBuffer(message.message);

        var id = bitBuffer.GetInt(0, 10);
                
        var x = bitBuffer.GetInt(0, 360);
        var y = bitBuffer.GetInt(0, 360);
        var z = bitBuffer.GetInt(0, 360);
                
        var h = bitBuffer.GetInt(-1, 1);
        var v = bitBuffer.GetInt(-1, 1);

        var shoot = bitBuffer.GetBit();
                
        if (message.messageType == MessageType.Input)
        {
            _players[id].playerMovement.Rotation(x, y, z);
            _players[id].playerMovement.Move(h, v);
            if (shoot)
            {
                _players[id].playerShooting.Shoot();
                _players[id].isShooting = true;
            }
        }

        if (_players[id].lastInputSequence < message.sequence)
        {
            _players[id].lastInputSequence = message.sequence;
        }
    }

    // Update is called once per frame
    public void Update()
    {

        if (Time.time < _timeForNextSnapshot)
        {
            Debug.Log(Time.time + " " + _timeForNextSnapshot);
            return;
        }

        Debug.Log("snapshot");

        _timeForNextSnapshot += 1f / _engine.serverSps;
        
        var data = _packetProcessor.GetData();
         while (data != null)
         {
            foreach (var message in data)
            {
                readMessage(message);
            }
            data = _packetProcessor.GetData();
         }

         BitBuffer bitBuffer = new BitBuffer();
         
         //TODO: cada uno tiene que tener su score
         bitBuffer.PutInt(score, 0, 100);
         
        foreach (var playersValue in _players.Values)
        {
            playersValue.GetPlayerState().serialize(bitBuffer);
        }

        foreach (var ipEndPoint in _ipEndPoints)
        {
            _packetProcessor.SendUnreliableData(bitBuffer.GetPayload(), ipEndPoint, MessageType.Snapshot, _sequence);
        }
        _sequence++;
    }

    public void playerDied(PlayerHealth playerHealth)
    {
        if (!playersHealth.Contains(playerHealth)) return;

        var index = playersHealth.IndexOf(playerHealth);
        playersHealth.Remove(playerHealth);
        playersTransforms.Remove(playersTransforms[index]);
        playersObjects.Remove(playersObjects[index]);
        
    }
}
