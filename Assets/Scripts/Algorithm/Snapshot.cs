﻿using System.Collections.Generic;

public class Snapshot
{
    public List<PlayerState> players = new List<PlayerState>();

    public int score;
    
    public int sequence;

    public float timestamp;
    
    public Snapshot(List<PlayerState>players, int sequence)
    {
        this.players = players;
        this.sequence = sequence;
    }

    public Snapshot()
    {
    }
}