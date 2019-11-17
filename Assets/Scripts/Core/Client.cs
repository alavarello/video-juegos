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

    private readonly Dictionary<int, ClientPlayer> _players;
    
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

    
    // Start is called before the first frame update
    public Client(Engine engine, ClientPlayer player)
    {
        _engine = engine;
        _serverIpEndPoint = new IPEndPoint(IPAddress.Parse(engine.serverIp), engine.serverListeningPort);
        _packetProcessor = new PacketProcessor(null, engine.clientListeningPort, true);
        _players = new Dictionary<int, ClientPlayer>();
        _interpolation = new Interpolation(engine);
        _prediction = new Prediction(player, _engine.playerId);
        _player = player;
        _floorMask = LayerMask.GetMask ("Floor");
        _healthSlider = GameObject.FindGameObjectsWithTag("Health")[0].GetComponent<Slider>();
        _damageImage = GameObject.FindGameObjectsWithTag("DamageImage")[0].GetComponent<Image>();
    }

    // Update is called once per frame
    
    public void Update()
    {
        if (Time.time < _timeForNextSnapshot) return;
        _timeForNextSnapshot += 1f / _engine.clientFps;
        

        if (_sequence != -1)
        {
            if (UpdateStates())
            {
                _sequence++;
                _timer += Time.deltaTime;
                if (!_player.isDead)
                {
                    SendMove();
                    SendRotation();
                    SendShot();
                    // Prediction
                    _player.UpdateHealth();
                    
                    _player.UpdateState();
                    _player.state.sequence = _sequence;
                    _prediction.AddState(_player.state);
                }
                
            }
        }
        var messages = _packetProcessor.GetData();
        if (messages == null) return;
        if (_sequence == -1)
        {
            var time = messages[0].sequence*(1.0f/_engine.serverSps);
            _sequence = Mathf.FloorToInt(time * _engine.clientFps);
        }
        foreach (var message in messages)
        {
            SaveMessage(message);
        }

    }


    private void SendRotation()
    {
        if (_playerRigidbody == null)
        {
            if (!_players.ContainsKey(_engine.playerId))
            {
                return;
            }

            _playerRigidbody = _players[_engine.playerId].GetComponent<Rigidbody>();
        }
        
        
        // Create a ray from the mouse cursor on screen in the direction of the camera.
        var camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.

        // Perform the raycast and if it hits something on the floor layer...
        if(!Physics.Raycast (camRay, out var floorHit, CamRayLength, _floorMask))
        {
            return;
        }
        // Create a vector from the player to the point on the floor thde raycast from the mouse hit.
        Vector3 playerToMouse = floorHit.point - _playerRigidbody.position;

        // Ensure the vector is entirely along the floor plane.
        playerToMouse.y = 0f;

        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation (playerToMouse);
        if (_playerRigidbody.transform.rotation == newRotation)
        {
            return;
        }
        var angles = newRotation.eulerAngles;
        IEnumerable<byte> concatenation = BitConverter.GetBytes(_engine.playerId)
            .Concat(BitConverter.GetBytes((int)angles.x))
            .Concat(BitConverter.GetBytes((int) angles.y))
            .Concat(BitConverter.GetBytes((int) angles.z));
        _packetProcessor.SendReliableFastData(concatenation.ToArray(), _serverIpEndPoint, MessageType.Rotation, _sequence);
        
        // Prediction
        _players[_engine.playerId].Rotation(angles.x, angles.y, angles.z);
    }
    
    private void SendMove()
    {
        // GetAxisRaw returns -1, 1, 0
        var h = (int) Input.GetAxisRaw("Horizontal");
        var v = (int) Input.GetAxisRaw("Vertical");

        if (v == 0 && h == 0)
        {
            return;
        }
        IEnumerable<byte> concatenation = BitConverter.GetBytes(_engine.playerId)
            .Concat(BitConverter.GetBytes(h))
            .Concat(BitConverter.GetBytes(v));
        _packetProcessor.SendReliableFastData(concatenation.ToArray(), _serverIpEndPoint, MessageType.Input, _sequence);
        
        // Prediction
       _player.Move(h, v);
    }
    
    private void SendShot()
    {
        // GetAxisRaw returns -1, 1, 0
        if (!Input.GetButton("Fire1") || !(_timer >= TimeBetweenBullets) || Time.timeScale == 0) return;
        
        
        _timer = 0;
        IEnumerable<byte> concatenation = BitConverter.GetBytes(_engine.playerId);
        _packetProcessor.SendReliableFastData(concatenation.ToArray(), _serverIpEndPoint, MessageType.Fire, _sequence);
        // Prediction
        _players[_engine.playerId].Shoot();

    }

    private void SaveMessage(Message message)
    {
        if (message != null)
        {
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            var score = BitConverter.ToInt32(message.message, 0);
            if(score > ScoreManager.score)
                ScoreManager.score = score;
            ms.Write(message.message, sizeof(Int32), message.message.Length-sizeof(Int32));
            ms.Seek(0, SeekOrigin.Begin);
            List<PlayerState> playerStates = (List<PlayerState>)bf.Deserialize(ms);
        
            _snapshot = new Snapshot(playerStates, message.sequence);
        
            _interpolation.AddSnapshot(_snapshot);
            
            _prediction.checkState(_snapshot);
        }
    }

    private bool UpdateStates()
    {
        var interpolatedSnapshot = _interpolation.Interpolate(_sequence);
        if (interpolatedSnapshot == null)
        {
            return false;
        }

        foreach (var playerState in interpolatedSnapshot.players)
        {
            if(!_players.ContainsKey(playerState.Id))
            {
                if (playerState.Id != _engine.playerId)
                {
                    var playerPrefab = Resources.Load<GameObject>("Prefabs/Client/ClientPlayer");
                    var player = GameObject.Instantiate<GameObject>(playerPrefab);  
                    var playerScript = player.GetComponent<ClientPlayer>();
                    _players.Add(playerState.Id, playerScript);
                }
                else
                {
                    _players.Add(playerState.Id, _player);
                    var camera = GameObject.Find("Main Camera");
                    camera.GetComponent<CameraFollow>().target = _player.transform;
                }

            }

            if (playerState.Id == _engine.playerId)
            {
                if (_players[playerState.Id].currentHealth != playerState.health)
                {
                    _healthSlider.value = playerState.health;
                    _damageImage.color = _flashColour;
                }
                else
                {
                    _damageImage.color = Color.Lerp (_damageImage.color, Color.clear, FlashSpeed * Time.deltaTime);
                }   
            }

            _players[playerState.Id].SetPlayerState(playerState); 
        }

        return true;

    }
}
