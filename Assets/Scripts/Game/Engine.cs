﻿﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Engine: MonoBehaviour
{
    public int serverListeningPort;
    
    public int clientListeningPort;
    
    public string serverIp;
    
    public bool isServer;

    public bool isClient;

    public int playerId;

    public int serverSps;

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
            EnemyMovement.engine = this;
            EnemyAttack.engine = this;
            ClientPlayer.playerId = playerId;
            client = new Client(this, FindObjectOfType<ClientPlayer>());
            ClientPlayer.client = client;
            GameOverManager.client = client;
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