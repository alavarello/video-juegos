using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Playables;

public class Server
{
    private readonly List<IPEndPoint> _ipEndPoints = new List<IPEndPoint>();

    private readonly PacketProcessor _packetProcessor;

    private readonly Dictionary<int, Player> _players;
    
    public static Dictionary<int, Enemy> enemies;
    
    public readonly List<int> deadIds = new List<int>();
    
    private readonly Engine _engine;

    private int _sequence;

    private float _timeForNextSnapshot;

    public static int score = 0;
    
    private int idCounter = 0;

    
    private Dictionary<int, int> _lastMessageReceived = new Dictionary<int, int>();
    

    public readonly List<Transform> playersTransforms = new List<Transform>();
    public readonly List<PlayerHealth> playersHealth = new List<PlayerHealth>();
    public readonly List<GameObject> playersObjects = new List<GameObject>();

    
    // Start is called before the first frame update
    public Server(Engine engine)
    {
        _engine = engine;
        _ipEndPoints = new List<IPEndPoint>();

        
        _packetProcessor = new PacketProcessor(null, engine.serverListeningPort, true);
        
        _players = new Dictionary<int, Player>();
        enemies = new Dictionary<int, Enemy>();
        
        
    }

    private static Player CreatePlayer()
    {
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Server/Player");
        var player = GameObject.Instantiate<GameObject>(playerPrefab);  
        var playerScript = player.GetComponent<Player>();
        return playerScript;
    }

    private void ReadMessage(Message message)
    {
        
        
        if (message.messageType == MessageType.Join)
        {
            var idPlayer = idCounter++;

            var playerScript = CreatePlayer();
            playerScript.id = idPlayer;
            playerScript.playerHealth = playerScript.GetComponent<PlayerHealth>();
            playerScript.playerMovement = playerScript.GetComponent<PlayerMovement>();
            _players.Add(idPlayer, playerScript);
            playersTransforms.Add(playerScript.playerMovement.transform);
            playersHealth.Add(playerScript.playerHealth);
            playersObjects.Add(playerScript.gameObject);

            _ipEndPoints.Add(message.from);

            BitBuffer buffer = new BitBuffer();
            buffer.InsertInt(idPlayer, 0, 10);

            _packetProcessor.SendReliableFastData(buffer.GetByteArray(), message.from, MessageType.JoinACK, _sequence);
            return;
        }
        
        BitBuffer bitBuffer = new BitBuffer(message.message);

        var id = bitBuffer.GetInt(0, 10);
        
        // Prevent ReliableFast double input problem
        if (_lastMessageReceived.ContainsKey(id) && _lastMessageReceived[id] >= message.messageId) return;

        _lastMessageReceived[id] = message.messageId;

        if (message.messageType == MessageType.Input)
        {
            var x = bitBuffer.GetInt(0, 360);
            var y = bitBuffer.GetInt(0, 360);
            var z = bitBuffer.GetInt(0, 360);
                
            var h = bitBuffer.GetInt(-1, 1);
            var v = bitBuffer.GetInt(-1, 1);

            var shoot = bitBuffer.GetBit();
            
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

    private void SendMessage()
    {
        BitBuffer bitBuffer = new BitBuffer();
         
        bitBuffer.InsertInt(score, 0, 10000);
        bitBuffer.InsertInt(LevelManager.level, 0, 10);
        bitBuffer.InsertInt(_players.Count, 0, 10);
         
        foreach (var playersValue in _players.Values)
        {
            playersValue.GetPlayerState().Serialize(bitBuffer);
        }

        bitBuffer.InsertInt(enemies.Count, 0, 1000);
        
        List<int> newDeadIds = new List<int>();
        foreach (var enemy in enemies.Values)
        {
            if (enemy._enemyHealth.isDead)
            {
                newDeadIds.Add(enemy.id);
            }
            enemy.GetEnemyState().Serialize(bitBuffer);
        }

        foreach (var id in newDeadIds)
        {
            if (deadIds.Contains(id))
                continue;
            enemies[id]._enemyHealth.DestroyGameObjectDestroy ();
            deadIds.Add(id);
        }
        
        // All the zombies are dead
        if (deadIds.Count == LevelManager.totalEnemies)
        {
            LevelManager.LevelUp();
            enemies = new Dictionary<int, Enemy>();
        }
        
        var payload = bitBuffer.GetByteArray();

        foreach (var ipEndPoint in _ipEndPoints)
        {
            _packetProcessor.SendUnreliableData(payload, ipEndPoint, MessageType.Snapshot, _sequence);
        }
    }

    public void Update()
    {
        if (Time.unscaledTime < _timeForNextSnapshot) return;
        
         _timeForNextSnapshot = Time.unscaledTime + (1f / _engine.serverSps);
        
         var data = _packetProcessor.GetData();
         while (data != null)
         {
            foreach (var message in data)
            {
                ReadMessage(message);
            }
            data = _packetProcessor.GetData();
         }
         
         SendMessage();
         
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
