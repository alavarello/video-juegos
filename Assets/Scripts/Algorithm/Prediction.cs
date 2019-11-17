
using System.Collections.Generic;
using UnityEngine;

public class Prediction
{
    private readonly SortedList<int, PlayerState> _states = new SortedList<int, PlayerState>();

    private readonly ClientPlayer _player;
    private readonly int _playerId;

    public Prediction(ClientPlayer player, int playerId)
    {
        _player = player;
        _playerId = playerId;
    }

    public void AddState(PlayerState playerState)
    {
        if (_states.Count != 0  && _states.Values[0].sequence >= playerState.sequence)
        {
            //Do nothing
            return;
        }

        if (_states.ContainsKey(playerState.sequence))
        {
            _states[playerState.sequence] = playerState;
        }
        else
        {
            _states.Add(playerState.sequence, playerState.Clone());
        }

    }

    public void checkState(Snapshot snapshot)
    {
        
        var playerState = snapshot.players[_playerId];
        if (!_states.ContainsKey(playerState.sequence)) return;

        // Check for position
        if (!_states[playerState.sequence].IsInTheSamePosition(playerState))
        {
            // Fix the player position (It will be updated later)
            _player.transform.position = new Vector3(playerState.x, 0 , playerState.z);

        }
        if (!_states[playerState.sequence].IsInTheSameRotation(playerState))
        {
            // Fix the player rotation (It will be updated later)
            _player.state.xA = playerState.xA;
            _player.state.zA = playerState.zA;
            _player.state.yA = playerState.yA;
        }

        // Remove the unused sequences
        var i = playerState.sequence;
        do
        {

            _states.Remove(i);
            i--;

        } while (_states.ContainsKey(i));
    }
    
   
}