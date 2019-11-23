﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interpolation
{
    private readonly Queue<Snapshot> _snapshots = new Queue<Snapshot>();

    private const int InterpolationSequence = 3;

    private Snapshot _fromSnapshot;
    private float _fromTime;
    
    private Snapshot _toSnapshot;
    private float _toTime;

    private readonly Engine _engine;

    private float baseTime = 0;
    private float previousTime;

    public Interpolation(Engine engine)
    {
        this._engine = engine;
    }


    public void AddSnapshot(Snapshot s)
    {
        if (_snapshots.Count == 0)
        {
            // Reset time
            baseTime = GetSnapshotTime(s.sequence);
            previousTime = Time.time;
        }
        if (_snapshots.Count != 0  && _snapshots.Peek().sequence >= s.sequence)
        {
            //Do nothing
            return;
        }

        if (_snapshots.Contains(s)) return;
       
       
        _snapshots.Enqueue(s);
        
    }

    private Snapshot GetSnapshotByExcess(float time)
    {
        foreach (var snapshot in _snapshots)
        {
            var snapshotTime = GetSnapshotTime(snapshot.sequence);
            if (snapshotTime > time)
            {
                return snapshot;
            }

        }

        return null;
    }

    private float GetSnapshotTime(int sequence)
    {
        return sequence / (float) _engine.serverSps;
        
    }

    private float GetClientTime()
    {

       
        baseTime += (Time.deltaTime);
        return baseTime;
    }
    
    private Snapshot GetSnapshotByDefect(float time)
    {
        if (_snapshots.Count < 2)
            return null;

        var firstTime = GetSnapshotTime(_snapshots.Peek().sequence);
        if (time < firstTime)
        {
            baseTime = firstTime;
            return null;
        }

        while (_snapshots.Count > 1)
        {
            var secondTime = GetSnapshotTime(_snapshots.ElementAt(1).sequence);
            if (time > secondTime)
                _snapshots.Dequeue();
            else
            {
                return _snapshots.Peek();
            }
        }

        if (_snapshots.Count == 0) return null;

        return _snapshots.Peek();
    }

    private Boolean CanInterpolate()
    {
        return _fromSnapshot != null && _toSnapshot != null;
    }

    private void interpolateEnemies(float interpolationTime, Snapshot interpolatedSnapshot)
    {
        var fromEnemies = _fromSnapshot.enemies;
        var toEnemies = _toSnapshot.enemies;
        var amountOfEnemies = fromEnemies.Count;
        
        for(var i = 0; i < amountOfEnemies; i++)
        {
            EnemyState fromEnemy = fromEnemies[i];
            EnemyState toEnemy;
            if (toEnemies.Count > i)
                toEnemy = toEnemies[i];
            else
                toEnemy = fromEnemy;
            var fromVector = new Vector3(fromEnemy.x, fromEnemy.y, fromEnemy.z);
            var toVector = new Vector3(toEnemy.x, toEnemy.y, toEnemy.z);
            var n = (interpolationTime - _fromTime) / (_toTime - _fromTime);
            var vector = Vector3.Lerp(fromVector, toVector, n);
            
            var fromRotateVector = new Vector3(fromEnemy.xA, fromEnemy.yA, fromEnemy.zA);
            var toRotateVector = new Vector3(toEnemy.xA, toEnemy.yA, toEnemy.zA);
            var rotation = Vector3.Lerp(fromRotateVector, toRotateVector, n);
            
            var state = new EnemyState(fromEnemy.id,
                vector.x, vector.y, vector.z,  
                rotation.x, rotation.y, rotation.z,
                fromEnemy.health
            );
            
            interpolatedSnapshot.enemies.Add(state);
        }
    }
    
    private void interpolatePlayers(float interpolationTime, Snapshot interpolatedSnapshot)
    {
        var from = _fromSnapshot.players;
        var to = _toSnapshot.players;

        var amountOfPlayers = Math.Min(from.Count, to.Count);
        
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
    }
    

    public Snapshot Interpolate()
    {
        var interpolationTime = baseTime;

        _toSnapshot = GetSnapshotByExcess(interpolationTime);
        _fromSnapshot = GetSnapshotByDefect(interpolationTime);
        if (!CanInterpolate() || _snapshots.Count < InterpolationSequence)
        {
            return null;
        }
        GetClientTime();
        
        _toTime = GetSnapshotTime(_toSnapshot.sequence);
        _fromTime = GetSnapshotTime(_fromSnapshot.sequence);
        
        var interpolatedSnapshot = new Snapshot();
        
        interpolatePlayers(interpolationTime, interpolatedSnapshot);
        interpolateEnemies(interpolationTime, interpolatedSnapshot);

        if(_fromSnapshot.score > ScoreManager.score)
            ScoreManager.score = _fromSnapshot.score;
        
        return interpolatedSnapshot;
    }
}