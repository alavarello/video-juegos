using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class Client
{
    private readonly PacketProcessor _packetProcessor;

    private readonly Engine _engine;

    private readonly IPEndPoint _serverIpEndPoint;

    public readonly Dictionary<int, ClientPlayer> players;
    
    public static Dictionary<int, ClientEnemy> enemies;
    
    private Snapshot _snapshot;

    private int _sequence = -1;

    private readonly Interpolation _interpolation;
    
    private readonly Prediction _prediction;

    private CameraFollow _cameraFollow;

    private readonly ClientPlayer _player;
    
    private Rigidbody _playerRigidbody;          // Reference to the player's rigidbody.
    
    private readonly int _floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.

    private const float CamRayLength = 100f;

    private const float FlashSpeed = 5f;
    
    private readonly Color _flashColour = new Color(1f, 0f, 0f, 0.1f);
    
    private readonly Slider _healthSlider;
    
    private readonly Image _damageImage;
    
    private const float TimeBetweenBullets = 0.15f;
    
    private float _timer;
    
    private float _timeForNextSnapshot = 0;

    public ClientPlayer cameraPlayer;

    
    // Start is called before the first frame update
    public Client(Engine engine, ClientPlayer player)
    {
        _engine = engine;
        _serverIpEndPoint = new IPEndPoint(IPAddress.Parse(engine.serverIp), engine.serverListeningPort);
        _packetProcessor = new PacketProcessor(null, engine.clientListeningPort, true);
        players = new Dictionary<int, ClientPlayer>();
        enemies = new Dictionary<int, ClientEnemy>();
        _interpolation = new Interpolation(engine);
        _prediction = new Prediction(player, _engine.playerId);
        _player = player;
        _floorMask = LayerMask.GetMask ("Floor");
        _healthSlider = GameObject.FindGameObjectsWithTag("Health")[0].GetComponent<Slider>();
        _damageImage = GameObject.FindGameObjectsWithTag("DamageImage")[0].GetComponent<Image>();
    }

    public void Update()
    {
        if (_sequence != -1)
        {
            UpdateStates();

            _sequence++;

            _timer += Time.deltaTime;
            if (!_player.isDead)
            {
                SendInput();

                // Prediction
                _player.UpdateHealth();
                _player.UpdateState();
                _player.Animating();
                _player.state.sequence = _sequence;
                _prediction.AddState(_player.state);
            }
        }

        var messages = _packetProcessor.GetData();
        while (messages != null)
        {
            if (_sequence == -1)
                _sequence = 1;
                    
            foreach (var message in messages)
                SaveMessage(message);

            messages = _packetProcessor.GetData();
        }
    }
    
    public void ChangeCamera()
    {
        var i = 0;
        foreach (var player in players.Values)
        {
            if(!player.isDead)
            {
                var camera = GameObject.Find("Main Camera");
                camera.GetComponent<CameraFollow>().target = player.transform;
                cameraPlayer = player;
            }
            
        }
    }

    private void SendInput()
    {
        var angles = GetRotation();
        var move = GetMove();
        var shoot = GetShot();
        
        _prediction.AddInputs(_sequence, (int)move.x, (int)move.y, (int)angles.y);
        BitBuffer bitBuffer = new BitBuffer();
        
        //TODO: cambiar rango de player id
        bitBuffer.PutInt(_engine.playerId, 0, 10);
        
        bitBuffer.PutInt((int)angles.x, 0, 360);
        bitBuffer.PutInt((int)angles.y, 0, 360);
        bitBuffer.PutInt((int)angles.z, 0, 360);
        
        bitBuffer.PutInt((int)move.x, -1, 1);
        bitBuffer.PutInt((int)move.y, -1, 1);
        
        bitBuffer.PutBit(shoot);
        
        _packetProcessor.SendReliableFastData(bitBuffer.GetPayload(), _serverIpEndPoint, MessageType.Input, _sequence);
    }

    private Vector3 GetRotation()
    {
        if (_playerRigidbody == null)
        {
            
            _playerRigidbody = _player.GetComponent<Rigidbody>();
        }
        
        var currentAngles = _playerRigidbody.transform.rotation.eulerAngles;
        
        // Create a ray from the mouse cursor on screen in the direction of the camera.
        var camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.

        // Perform the raycast and if it hits something on the floor layer...
        if(!Physics.Raycast (camRay, out var floorHit, CamRayLength, _floorMask))
        {
            return currentAngles;
        }
        // Create a vector from the player to the point on the floor thde raycast from the mouse hit.
        Vector3 playerToMouse = floorHit.point - _playerRigidbody.position;

        // Ensure the vector is entirely along the floor plane.
        playerToMouse.y = 0f;

        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation (playerToMouse);
        
        if (_playerRigidbody.transform.rotation == newRotation)
        {
            return currentAngles;
        }
        var angles = newRotation.eulerAngles;

        // Prediction
        _player.Rotation(angles.x, angles.y, angles.z);
        
        return angles;
    }
    
    private Vector2 GetMove()
    {
        // GetAxisRaw returns -1, 1, 0
        var h = (int) Input.GetAxisRaw("Horizontal");
        var v = (int) Input.GetAxisRaw("Vertical");

        if (v == 0 && h == 0)
        {
            return Vector2.zero;
        }

        // Prediction
       _player.Move(h, v);
       return new Vector2(h, v);
    }
    
    private bool GetShot()
    {
        // GetAxisRaw returns -1, 1, 0
        if (!Input.GetButton("Fire1") || !(_timer >= TimeBetweenBullets) || Math.Abs(Time.timeScale) < 0.0001) return false;
        
        _timer = 0;
        
        // Prediction
        _player.Shoot();

        return true;
    }

    private void SaveMessage(Message message)
    {
        if (message == null) return;
        
        var bitBuffer = new BitBuffer(message.message);

        var score = bitBuffer.GetInt(0, 1000);
        var playerCount = bitBuffer.GetInt(0, 10);

        var playerStates = new List<PlayerState>();
        var enemyStates = new List<EnemyState>();

        for (var i = 0; i < playerCount; i++)
        {
            playerStates.Add(new PlayerState(bitBuffer));
        }

        var enemyCount = bitBuffer.GetInt(0, 100);
        for (var i = 0; i < enemyCount; i++)
        {
            enemyStates.Add(new EnemyState(bitBuffer));
        }
        
        _snapshot = new Snapshot(playerStates, enemyStates, message.sequence);
        _snapshot.score = score;
            
        _interpolation.AddSnapshot(_snapshot);
            
        _prediction.checkState(_snapshot);
    }

    private void UpdatePlayers(Snapshot interpolatedSnapshot)
    {
        foreach (var playerState in interpolatedSnapshot.players)
        {
            if(!players.ContainsKey(playerState.Id))
            {
                if (playerState.Id != _engine.playerId)
                {
                    var playerPrefab = Resources.Load<GameObject>("Prefabs/Client/ClientPlayer");
                    var player = GameObject.Instantiate(playerPrefab);  
                    var playerScript = player.GetComponent<ClientPlayer>();
                    players.Add(playerState.Id, playerScript);
                }
                else
                {
                    players.Add(playerState.Id, _player);
                    var camera = GameObject.Find("Main Camera");
                    camera.GetComponent<CameraFollow>().target = _player.transform;
                    cameraPlayer = _player;
                }
            }

            if (playerState.Id == _engine.playerId)
            {
                if (players[playerState.Id].currentHealth != playerState.health)
                {
                    _healthSlider.value = playerState.health;
                    _damageImage.color = _flashColour;
                }
                else
                {
                    _damageImage.color = Color.Lerp (_damageImage.color, Color.clear, FlashSpeed * Time.deltaTime);
                }   
            }

            players[playerState.Id].SetPlayerState(playerState); 
        }
    }
    
    private void UpdateEnemies(Snapshot interpolatedSnapshot)
    {
        foreach (var enemyState in interpolatedSnapshot.enemies)
        {
            if(!enemies.ContainsKey(enemyState.id))
            {
                GameObject enemyPrefab = null;
                switch (enemyState.type)
                {
                    case EnemyType.ZomBunny:
                        enemyPrefab = Resources.Load<GameObject>("Prefabs/Client/ZomBunny");
                        break;
                    case EnemyType.ZomBear:
                        enemyPrefab = Resources.Load<GameObject>("Prefabs/Client/ZomBear");
                        break;
                    case EnemyType.Hellephants:
                        enemyPrefab = Resources.Load<GameObject>("Prefabs/Client/Hellephants");
                        break;

                }
                var enemy = GameObject.Instantiate(enemyPrefab);  
                var enemyScript = enemy.GetComponent<ClientEnemy>();
                enemyScript.state = enemyState;
                enemyScript.transform.position = new Vector3(enemyState.x, 0, enemyState.z);
                enemies.Add(enemyState.id, enemyScript);
            }
            else
            {
                var clientEnemy = enemies[enemyState.id];
                if (clientEnemy.state.health != enemyState.health)
                {
                    clientEnemy.TakeDamage(new Vector3(enemyState.xH, enemyState.yH, enemyState.zH), enemyState.health);
                }

                clientEnemy.state = enemyState;
            }
        }
    }

    private void UpdateStates()
    {
        var interpolatedSnapshot = _interpolation.Interpolate();
        if (interpolatedSnapshot == null) return;

        UpdatePlayers(interpolatedSnapshot);
        UpdateEnemies(interpolatedSnapshot);
    }
}
