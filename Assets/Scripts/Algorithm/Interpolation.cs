﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interpolation
{
    private SortedList<int, Snapshot> _snapshots = new SortedList<int, Snapshot>();

    private int _interpolationSequence = 5;

    private Snapshot fromSnapshot;
    private float fromTime;
    
    private Snapshot toSnapshot;
    private float toTime;

    private Engine engine;

    private bool _isInterpolationTime = true;


    public Interpolation(Engine engine)
    {
        this.engine = engine;
    }

    public void AddSnapshot(Snapshot s)
    {
        if (_snapshots.Count != 0  && _snapshots.Values[0].sequence >= s.sequence)
        {
            //Do nothing
            return;
        }
        _snapshots.Add(s.sequence, s);
    }



//        if (s.sequence > _interpolationSequence)
//        {
//            _isInterpolationTime = true;
//            _interpolationSequence += 5;
//        }
//        _snapshots.Enqueue(s);
    

    public Snapshot GetSnapshotByExcess(float time)
    {
        foreach (var snapshot in _snapshots)
        {
            float snapshotTime = GetSnapshotTime(snapshot.Key);
            if (snapshotTime > time)
            {
                return snapshot.Value;
            }

        }

        return null;
    }

    public float GetSnapshotTime(int sequence)
    {
        return sequence / (float) engine.serverSps;
        
    }

    public float GetClientTime(int sequence)
    {
        return sequence / (float) engine.clientFps;
    }

    public Snapshot GetSnapshotByDefect(float time)
    {
        bool flag = false;
        Snapshot lastSnapshot = null;
        foreach (var snapshot in _snapshots)
        {
            float snapshotTime = GetSnapshotTime(snapshot.Key);
            
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

    public Boolean CanInterpolate()
    {
        return fromSnapshot != null && toSnapshot != null;
    }
    

    public Snapshot Interpolate(int clientSequence)
    {
        
        float interpolationTime = GetClientTime(clientSequence);
        
        
        var currentFromSnapshot = GetSnapshotByDefect(interpolationTime);
        if (currentFromSnapshot != fromSnapshot && fromSnapshot != null)
        {
            _snapshots.Remove(fromSnapshot.sequence);

        }
        fromSnapshot = currentFromSnapshot;
        
        toSnapshot = GetSnapshotByExcess(interpolationTime);
        if (!CanInterpolate() || _snapshots.Count < 5)
        {
            return null;
        }

        toTime = GetSnapshotTime(toSnapshot.sequence);
        fromTime = GetSnapshotTime(fromSnapshot.sequence);

        var from = fromSnapshot.players;
        var to = toSnapshot.players;
        var amountOfPlayers = Math.Min(from.Count, to.Count);
        
        var interpolatedSnapshot = new Snapshot();
        for(var i = 0; i < amountOfPlayers; i++)
        {
            var fromVector = new Vector3(from[i].x, from[i].y, from[i].z);
            var toVector = new Vector3(to[i].x, to[i].y, to[i].z);
            var n = (interpolationTime - fromTime) / (toTime - fromTime);
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
        
        return interpolatedSnapshot;
        
//        
//        var clientTime = clientSequence / (float) engine.clientFps;
//        if (fromSnapshot == null)
//        {
//            fromSnapshot = _snapshots.Dequeue();
//        }
//        if (toTime < clientTime)
//        {
//            fromSnapshot = _snapshots.Dequeue();
//
//        }
//        toSnapshot = _snapshots.Peek();
//        toTime = toSnapshot.sequence / (float) engine.serverSps;
        
       
    }
}