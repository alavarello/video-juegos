﻿using System.Collections.Generic;

public class Snapshot
{
    public List<PlayerState> players = new List<PlayerState>();
    
    public List<EnemyState> enemies = new List<EnemyState>();

    public int score;
    
    public int sequence;

    public float timestamp;
    
    public Snapshot(List<PlayerState>players, int sequence)
    {
        this.players = players;
        this.sequence = sequence;
    }
    
    public Snapshot(List<PlayerState>players, List<EnemyState> enemies, int sequence)
    {
        this.players = players;
        this.enemies = enemies;
        this.sequence = sequence;
    }

    public Snapshot()
    {
    }
}