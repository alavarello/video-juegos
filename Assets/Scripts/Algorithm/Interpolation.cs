﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interpolation
{
    private readonly Queue<Snapshot> _snapshots;
    private int lastSequence = 0;
    protected readonly int window;
    private const int InterpolationSequence = 3;
    protected float baseTime;
    protected float timestamp;

    private Snapshot _fromSnapshot;
    private float _fromTime;
    
    private Snapshot _toSnapshot;
    private float _toTime;

    private readonly Engine _engine;

    private float _time = 0f;

    public Interpolation(Engine engine)
    {
        _engine = engine;
        _snapshots = new Queue<Snapshot>();
        window = 3;
        baseTime = -1.0f;
        timestamp = -1.0f;
        lastSequence = -1;
        _fromSnapshot = null;
        _toSnapshot = null;
    }

    public void AddSnapshot(Snapshot s)
    {
        if (lastSequence < s.sequence) {
            if (lastSequence < 0) {
                baseTime = Time.unscaledTime - GetSnapshotTime(s.sequence);
            }
            lastSequence = s.sequence;
            _snapshots.Enqueue(s);
        }
        else {
            // La secuencia es antigua, y el paquete se descarta.
        }
        return;
    }


    private Snapshot GetSnapshotByExcess(float time)
    {
        Snapshot excessSnapshot = null;
        while (0 < _snapshots.Count) {
            Snapshot snapshot = _snapshots.Peek();
            float t = GetSnapshotTime(snapshot.sequence);
            if (time < t) {
                excessSnapshot = snapshot;
                _snapshots.Dequeue();
                break;
            }
            else _snapshots.Dequeue();
        }
        return excessSnapshot;
    }

    private float GetSnapshotTime(int sequence)
    {
        return sequence / (float) _engine.serverSps;
        
    }
    
    public float GetInterpolationTime() {
        return Time.unscaledTime - baseTime - (1f/_engine.serverSps);
    }
    
    protected void TrySetTimestamp() {
        if (CanInterpolate() && timestamp < 0) {
            timestamp = GetInterpolationTime();
        }
        return;
    }
    
    protected void SwitchSnapshots() {
        timestamp = -1.0f;
        _fromSnapshot = _toSnapshot;
        _toSnapshot = null;
        SlideWindow();
        TrySetTimestamp();
    }
    
    protected void SlideWindow() {
        float t = GetInterpolationTime();
        if (_fromSnapshot == null) {
            _fromSnapshot = GetSnapshotByDefect(t);
        }
        else {
            float fromTime = GetSnapshotTime(_fromSnapshot.sequence);
            if (fromTime <= t && t < fromTime + (1f/_engine.serverSps)) {
                // El tiempo por defecto (from), no venció.
            }
            else {
                SwitchSnapshots();
            }
        }
        if (_fromSnapshot != null) {
            if (_toSnapshot == null) {
                _toSnapshot = GetSnapshotByExcess(t);
            }
            else {
                float toTime = GetSnapshotTime(_toSnapshot.sequence);
                if (t < toTime) {
                    // El tiempo por exceso (to), no venció.
                }
                else {
                    _toSnapshot = GetSnapshotByExcess(t);
                }
            }
        }
        return;
    }


    private float GetClientTime(int sequence)
    {
        return sequence / (float) _engine.clientFps;
    }

    private Snapshot GetSnapshotByDefect(float time)
    {
        Snapshot defaultSnapshot = null;
        while (0 < _snapshots.Count) {
            Snapshot snapshot = _snapshots.Peek();
            float t = GetSnapshotTime(snapshot.sequence);
            if (t <= time) {
                defaultSnapshot = snapshot;
                _snapshots.Dequeue();
            }
            else break;
        }
        return defaultSnapshot;
    }

    private Boolean CanInterpolate()
    {
        return _fromSnapshot != null && _toSnapshot != null;
    }
    
    protected float GetInterpolationDelta() {
        return GetInterpolationTime() - timestamp;
    }

    private Snapshot InterpolateSnapshot(Snapshot from, Snapshot to, float deltaN)
    {
        
        var sequence = from.sequence;
        timestamp = from.timestamp + deltaN * (to.timestamp - from.timestamp);
        var fromPlayers = _fromSnapshot.players;
        var toPlayers = _toSnapshot.players;
        var amountOfPlayers = Math.Min(fromPlayers.Count, toPlayers.Count);
        
        var interpolatedSnapshot = new Snapshot();
        for(var i = 0; i < amountOfPlayers; i++)
        {
            var fromVector = new Vector3(fromPlayers[i].x, fromPlayers[i].y, fromPlayers[i].z);
            var toVector = new Vector3(toPlayers[i].x, toPlayers[i].y, toPlayers[i].z);
            var vector = Vector3.Lerp(fromVector, toVector, deltaN);
            var fromRotateVector = new Vector3(fromPlayers[i].xA, fromPlayers[i].yA, fromPlayers[i].zA);
            var toRotateVector = new Vector3(toPlayers[i].xA, toPlayers[i].yA, toPlayers[i].zA);
            var rotation = Vector3.Lerp(fromRotateVector, toRotateVector, deltaN);
            var cubeState = new PlayerState(
                vector.x, vector.y, vector.z, 
                rotation.x, rotation.y, rotation.z, 
                fromPlayers[i].health,
                fromPlayers[i].isShooting
            );
            cubeState.Id = fromPlayers[i].Id;
            interpolatedSnapshot.players.Add(cubeState);
        }

        interpolatedSnapshot.score = _fromSnapshot.score;

        
        return new Snapshot();
    }

    
    public Snapshot Interpolate(int clientSequence)
    {
        Snapshot interpolated = null;
        SlideWindow();
        TrySetTimestamp();
        if (CanInterpolate()) {
            float Δt = GetInterpolationDelta();
            float Δn = Δt/(1f/_engine.serverSps);
            interpolated = InterpolateSnapshot(_fromSnapshot, _toSnapshot, Δn);
        }
        return interpolated;
    }
}