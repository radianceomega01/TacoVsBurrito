using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public class RoomManager : NetworkBehaviour
    {

        [Networked, Capacity(4)]
        public NetworkLinkedList<PlayerData> Players => default;


        public event Action<List<PlayerData>> OnRoomPlayersUpdated;

        public override void Spawned()
        {
            NotifyPlayersUpdated();
        }


        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined {player}");

            if (runner.IsServer)
            {
                var data = new PlayerData(
                    player
                );

                Players.Add(data);
            }

            NotifyPlayersUpdated();
        }


        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                foreach (var p in Players)
                {
                    if (p.Player == player)
                    {
                        Players.Remove(p);
                        break;
                    }
                }
            }

            NotifyPlayersUpdated();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetPlayerName(NetworkString<_32> playerName, RpcInfo info = default)
        {
            if (!Runner.IsServer)
                return;


            for (int i = 0; i < Players.Count; i++)
            {
                var data = Players[i];

                if (data.Player == info.Source)
                {
                    data.Name = playerName;

                    Players.Set(i, data);

                    break;
                }
            }

            NotifyPlayersUpdated();
        }


        private void NotifyPlayersUpdated()
        {
            List<PlayerData> currentPlayers = new();

            foreach (var player in Players)
            {
                currentPlayers.Add(player);
            }

            OnRoomPlayersUpdated?.Invoke(currentPlayers);
        }
    }

    public struct PlayerData : INetworkStruct
    {
        public PlayerRef Player;
        public NetworkString<_32> Name;

        public PlayerData(PlayerRef player)
        {
            Player = player;
            Name = string.Empty;
        }
    }
}