using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Client
{
    private PacketProcessor _packetProcessor;

    private Engine _engine;

    private IPEndPoint serverIpEndPoint;

    private Dictionary<int, ClientPlayer> players;
    
    private Snapshot snapshot;

    private int sequence = -1;

    private Interpolation _interpolation;

    private CameraFollow _cameraFollow;

    private ClientPlayer _player;
    
    private Rigidbody _playerRigidbody;          // Reference to the player's rigidbody.
    
    private int _floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    
    private float _camRayLength = 100f;

    public float flashSpeed = 5f;
    
    public Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    
    public Slider healthSlider;
    
    public Image damageImage;
    
    public float timeBetweenBullets = 0.15f;
    
    float timer;
    
    // Start is called before the first frame update
    public Client(Engine engine, ClientPlayer player)
    {
        _engine = engine;
        serverIpEndPoint = new IPEndPoint(IPAddress.Parse(engine.serverIp), engine.serverListeningPort);
        _packetProcessor = new PacketProcessor(null, engine.clientListeningPort, true);
        players = new Dictionary<int, ClientPlayer>();
        _interpolation = new Interpolation(engine);
        _player = player;
        _floorMask = LayerMask.GetMask ("Floor");
        healthSlider = GameObject.FindGameObjectsWithTag("Health")[0].GetComponent<Slider>();
        damageImage = GameObject.FindGameObjectsWithTag("DamageImage")[0].GetComponent<Image>();
    }

    // Update is called once per frame
    
    public void Update()
    {
        if (sequence != -1)
        {
            if (UpdateStates())
            {
                sequence++;
                timer += Time.deltaTime;

            }
        }
        var messages = _packetProcessor.GetData();
        if (messages == null) return;
        if (sequence == -1)
        {
            var time = messages[0].sequence*(1.0f/_engine.serverSps);
            sequence = Mathf.FloorToInt(time * _engine.clientFps);
        }
        foreach (var message in messages)
        {
            SaveMessage(message);
        }
    }

    public void FixedUpdate()
    {
        if (sequence % 2 == 0)
        {
            SendMove();
            SendRotation();
            SendShot();
        }
    }

    private void SendRotation()
    {
        if (_playerRigidbody == null)
        {
            if (!players.ContainsKey(_engine.playerId))
            {
                return;
            }

            _playerRigidbody = players[_engine.playerId].GetComponent<Rigidbody>();
        }
        
        
        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

        // Create a RaycastHit variable to store information about what was hit by the ray.
        RaycastHit floorHit;

        // Perform the raycast and if it hits something on the floor layer...
        if(!Physics.Raycast (camRay, out floorHit, _camRayLength, _floorMask))
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
        _packetProcessor.SendReliableFastData(concatenation.ToArray(), serverIpEndPoint, MessageType.Rotation, sequence);
    }
    
    private void SendMove()
    {
        // GetAxisRaw returns -1, 1, 0
        int h = (int) Input.GetAxisRaw("Horizontal");
        int v = (int) Input.GetAxisRaw("Vertical");

        if (v == 0 && h == 0)
        {
            return;
        }
        IEnumerable<byte> concatenation = BitConverter.GetBytes(_engine.playerId)
            .Concat(BitConverter.GetBytes(h))
            .Concat(BitConverter.GetBytes(v));
        _packetProcessor.SendReliableFastData(concatenation.ToArray(), serverIpEndPoint, MessageType.Input, sequence);
    }
    
    private void SendShot()
    {
        // GetAxisRaw returns -1, 1, 0
        if (Input.GetButton("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            timer = 0;
            IEnumerable<byte> concatenation = BitConverter.GetBytes(_engine.playerId);
            _packetProcessor.SendReliableFastData(concatenation.ToArray(), serverIpEndPoint, MessageType.Fire, sequence);
        }
    }

    private void SaveMessage(Message message)
    {
        if (message != null)
        {
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            ms.Write(message.message, 0, message.message.Length);
            ms.Seek(0, SeekOrigin.Begin);
            List<PlayerState> playerStates = (List<PlayerState>)bf.Deserialize(ms);
        
            snapshot = new Snapshot(playerStates, message.sequence);
        
            _interpolation.AddSnapshot(snapshot);
        }
    }

    private bool UpdateStates()
    {

        var interpolatedSnapshot = _interpolation.Interpolate(sequence);

        if (interpolatedSnapshot == null)
        {
            return false;
        }
        
        foreach (var playerState in interpolatedSnapshot.players)
        {
            if(!players.ContainsKey(playerState.Id))
            {
                GameObject player;
                if (playerState.Id != _engine.playerId)
                {
                    var playerPrefab = Resources.Load<GameObject>("Prefabs/Client/ClientPlayer");
                    player = GameObject.Instantiate<GameObject>(playerPrefab);  
                    var playerScript = player.GetComponent<ClientPlayer>();
                    players.Add(playerState.Id, playerScript);
                }
                else
                {
                    players.Add(playerState.Id, _player);
                    var camera = GameObject.Find("Main Camera");
                    camera.GetComponent<CameraFollow>().target = _player.transform;
                }

            }

            if (players[playerState.Id].currentHealth != playerState.health)
            {
                healthSlider.value = playerState.health;
                Debug.Log(playerState.health);
                damageImage.color = flashColour;
            }
            else
            {
                damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }
            
            players[playerState.Id].SetPlayerState(playerState); 
        }

        return true;

    }
}
