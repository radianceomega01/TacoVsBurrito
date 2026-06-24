using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class RoomManager : NetworkBehaviour
    {

        [Networked, Capacity(4)]
        public NetworkLinkedList<PlayerData> Players => default;

        [Networked, OnChangedRender(nameof(OnPlayersChanged))]
        public int PlayersUpdateVersion { get; set; }


        public bool IsSpawned { get; private set; }
        public event Action<List<PlayerData>> OnRoomPlayersUpdated;

        public override void Spawned()
        {
            IsSpawned = true;
            FusionNetworkManager.Instance.OnPlayerJoinedEvent += ManagePlayerJoined;
            FusionNetworkManager.Instance.OnPlayerLeftEvent += ManagePlayerLeft;

            //AddPlayersInitially();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            IsSpawned = false;
            FusionNetworkManager.Instance.OnPlayerJoinedEvent -= ManagePlayerJoined;
            FusionNetworkManager.Instance.OnPlayerLeftEvent -= ManagePlayerLeft;
        }

        private void AddPlayersInitially()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                var data = new PlayerData(
                    player
                );

                Players.Add(data);
            }
            OnPlayersChanged();
        }

        void ManagePlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined {player}");

            if (runner.IsServer)
            {
                var data = new PlayerData(
                    player
                );

                Players.Add(data);
                PlayersUpdateVersion++;
                Debug.LogWarning($"PlayersUpdateVersion = {PlayersUpdateVersion}");
            }

        }

        void ManagePlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                foreach (var p in Players)
                {
                    if (p.Player == player)
                    {
                        Players.Remove(p);
                        PlayersUpdateVersion++;
                        break;
                    }
                }
            }
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
                    PlayersUpdateVersion++;
                    break;
                }
            }
        }

        void OnPlayersChanged()
        {
            Debug.LogWarning("invoking event players updated");
            OnRoomPlayersUpdated?.Invoke(Players.ToList());
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