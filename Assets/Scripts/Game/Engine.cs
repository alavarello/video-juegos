﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Engine: MonoBehaviour
{
    public int serverListeningPort;
    
    public int clientListeningPort;

    public List<string> IPs = new List<string>();

    public string serverIp;
    
    public bool isServer;

    public bool isClient;

    public int playerId;

    public int serverSps;

    public int clientFps;

    public Server server;

    public Client client;

    private void Start()
    {
        if (isServer)
        {
            EnemyMovement.engine = this;
            EnemyAttack.engine = this;
            PlayerHealth.engine = this;
            server = new Server(this);
        }

        if(isClient)
        {
            ClientPlayer.playerId = playerId;
                client = new Client(this, FindObjectOfType<ClientPlayer>());
        }
    }

    private void Update()
    {
        if (isClient)
        {
            client.Update();
        }

        if (isServer)
        {
            server.Update();
        }
    }
}