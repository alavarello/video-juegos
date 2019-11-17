﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interpolation
{
    private readonly SortedList<int, Snapshot> _snapshots = new SortedList<int, Snapshot>();

    private const int InterpolationSequence = 5;

    private Snapshot _fromSnapshot;
    private float _fromTime;
    
    private Snapshot _toSnapshot;
    private float _toTime;

    private readonly Engine _engine;
    

    public Interpolation(Engine engine)
    {
        this._engine = engine;
    }

    public void AddSnapshot(Snapshot s)
    {
        if (_snapshots.Count != 0  && _snapshots.Values[0].sequence >= s.sequence)
        {
            //Do nothing
            return;
        }

        if (_snapshots.ContainsKey(s.sequence))
        {
            _snapshots[s.sequence] = s;
        }
        else
        {
            _snapshots.Add(s.sequence, s);
        }
    }


    private Snapshot GetSnapshotByExcess(float time)
    {
        foreach (var snapshot in _snapshots)
        {
            var snapshotTime = GetSnapshotTime(snapshot.Key);
            if (snapshotTime > time)
            {
                return snapshot.Value;
            }

        }

        return null;
    }

    private float GetSnapshotTime(int sequence)
    {
        return sequence / (float) _engine.serverSps;
        
    }

    private float GetClientTime(int sequence)
    {
        return sequence / (float) _engine.clientFps;
    }

    private Snapshot GetSnapshotByDefect(float time)
    {
        var flag = false;
        Snapshot lastSnapshot = null;
        foreach (var snapshot in _snapshots)
        {
            var snapshotTime = GetSnapshotTime(snapshot.Key);
            
            if (snapshotTime <= time)
            {
                lastSnapshot = snapshot.Value;
                flag = true;
            }

            if (snapshotTime > time && flag)
            {
                return lastSnapshot;
            }

        }

        return null;
    }

    private Boolean CanInterpolate()
    {
        return _fromSnapshot != null && _toSnapshot != null;
    }
    

    public Snapshot Interpolate(int clientSequence)
    {
        
        var interpolationTime = GetClientTime(clientSequence);
        
        
        var currentFromSnapshot = GetSnapshotByDefect(interpolationTime);
        if (currentFromSnapshot != _fromSnapshot && _fromSnapshot != null)
        {
            _snapshots.Remove(_fromSnapshot.sequence);

        }
        _fromSnapshot = currentFromSnapshot;
        
        _toSnapshot = GetSnapshotByExcess(interpolationTime);
        if (!CanInterpolate() || _snapshots.Count < InterpolationSequence)
        {
            return null;
        }

        _toTime = GetSnapshotTime(_toSnapshot.sequence);
        _fromTime = GetSnapshotTime(_fromSnapshot.sequence);
        var from = _fromSnapshot.players;
        var to = _toSnapshot.players;
        var amountOfPlayers = Math.Min(from.Count, to.Count);
        
        var interpolatedSnapshot = new Snapshot();
        for(var i = 0; i < amountOfPlayers; i++)
        {
            var fromVector = new Vector3(from[i].x, from[i].y, from[i].z);
            var toVector = new Vector3(to[i].x, to[i].y, to[i].z);
            var n = (interpolationTime - _fromTime) / (_toTime - _fromTime);
            var vector = Vector3.Lerp(fromVector, toVector, n);
            var fromRotateVector = new Vector3(from[i].xA, from[i].yA, from[i].zA);
            var toRotateVector = new Vector3(to[i].xA, to[i].yA, to[i].zA);
            var rotation = Vector3.Lerp(fromRotateVector, toRotateVector, n);
            var cubeState = new PlayerState(
                vector.x, vector.y, vector.z, 
                rotation.x, rotation.y, rotation.z, 
                from[i].health,
                from[i].isShooting
                );
            cubeState.Id = from[i].Id;
            interpolatedSnapshot.players.Add(cubeState);
        }

        interpolatedSnapshot.score = _fromSnapshot.score;
        
        return interpolatedSnapshot;
        
//       

    }
}