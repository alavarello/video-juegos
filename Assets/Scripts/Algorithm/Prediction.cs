
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Prediction
{

    private class PredictionVariables
    {
        public int h, v, yA, sequence;

        public PredictionVariables(int h, int v, int yA, int sequence)
        {
            this.h = h;
            this.v = v;
            this.yA = yA;
            this.sequence = sequence;

        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return ((PredictionVariables) obj).sequence == sequence;
        }

    }
    
    private readonly Queue<PlayerState> _states = new Queue<PlayerState>();
    private readonly Queue<PredictionVariables> inputs = new Queue<PredictionVariables>();


    private readonly ClientPlayer _player;
    private readonly int _playerId;

    public Prediction(ClientPlayer player, int playerId)
    {
        _player = player;
        _playerId = playerId;
    }

    public void AddInputs(int sequence, int h, int v, int yA)
    {
        inputs.Enqueue(new PredictionVariables(h, v, yA, sequence));
    }

    public void RebuildSequence(PlayerState playerState)
    {
        _states.Clear();
        foreach (var input in inputs)
        {
            var movment = new Vector3(input.h, 0, input.v);
            movment = movment.normalized * 0.2f;
            _states.Enqueue(new PlayerState(playerState.x + movment.x, 0, playerState.z + movment.z, 0, playerState.y, 0, 0, false));
        }

    }

    public void AddState(PlayerState playerState)
    {
        if (_states.Count != 0  && _states.Peek().sequence >= playerState.sequence)
        {
            //Do nothing
            return;
        }

        if (_states.Contains(playerState)) return;
       
        _states.Enqueue(playerState.Clone());
   

    }

    private PlayerState GetPlayerState(int sequence)
    {
        var playerState = _states.Peek();
        while (sequence > playerState.sequence)
            playerState = _states.Dequeue();
        var input = inputs.Peek();
        while (sequence > input.sequence)
            input = inputs.Dequeue();

        return playerState.sequence != sequence ? null : playerState;
    }

    public void checkState(Snapshot snapshot)
    {
        var snapshotState = snapshot.players[_playerId];
        var playerState = GetPlayerState(snapshotState.sequence);
        if (playerState == null) return;
        
        // Check for position
        if (!playerState.IsInTheSamePosition(snapshotState))
        {
            // Fix the player position (It will be updated later)
            _player.transform.position = new Vector3(snapshotState.x, 0 , snapshotState.z);
            RebuildSequence(snapshotState);

        }
        if (!playerState.IsInTheSameRotation(snapshotState))
        {
            // Fix the player rotation (It will be updated later)
            _player.state.xA = snapshotState.xA;
            _player.state.zA = snapshotState.zA;
            _player.state.yA = snapshotState.yA;
        }
        
    }
    
   
}