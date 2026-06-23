using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using System;
using Unity.VectorGraphics;

namespace TacoVsBurrito
{
    public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static FusionNetworkManager Instance;

        public NetworkRunner Runner { get; private set; }
        public List<SessionInfo> AvailableSessions { get; private set; }

        NetworkSceneManagerDefault sceneManager;

        public event Action<NetworkRunner, string> OnJoinLobbyFailedEvent;
        public event Action<NetworkRunner, NetDisconnectReason> OnDisconnectedFromServerEvent;
        public event Action<NetworkRunner, PlayerRef> OnPlayerJoinedEvent;
        public event Action<string> OnJoinFailedEvent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            Init();
        }

        void Init()
        {
            EnsureRunner();
            EnsureSceneManager();
        }

        private void EnsureRunner()
        {
            if (Runner != null)
                return;

            Runner = gameObject.AddComponent<NetworkRunner>();
            Runner.ProvideInput = true;

            Runner.AddCallbacks(this); 
        }

        private void EnsureSceneManager()
        {
            if (sceneManager == null)
                sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        public async void JoinLobby()
        {
            var result = await Runner.JoinSessionLobby(SessionLobby.ClientServer);
            if (result.Ok)
            {
                JoinOnlineGame(Runner.SessionInfo.Name);
            }
            else
            {
                OnJoinLobbyFailedEvent?.Invoke(Runner, result.ErrorMessage);
            }
        }

        public async void CreateFriendRoom(int playerCount)
        {
            var result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = GenerateRoomCode(),
                SceneManager = sceneManager,
                PlayerCount = playerCount
            });
            InvokeRoomJoinedEvent(result);
            Debug.Log($"Host Result: {result.Ok}");
        }

        public async void JoinFriendRoom(string roomCode)
        {
            var result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = roomCode,
                SceneManager = sceneManager
            });
            InvokeRoomJoinedEvent(result);
            Debug.Log($"Join Result: {result.Ok}");
        }


        async void JoinOnlineGame(string sessionName)
        {
            var result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = sessionName,
                SceneManager = sceneManager
            });
            InvokeRoomJoinedEvent(result);
        }


        public void OnSessionListUpdated(
            NetworkRunner runner,
            List<SessionInfo> sessionList)
        {
            AvailableSessions = sessionList;

            Debug.Log($"Sessions Found: {sessionList.Count}");

            foreach (var session in sessionList)
            {
                Debug.Log(session.Name);
            }
        }

        string GenerateRoomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            System.Random random = new System.Random();
            char[] code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }

            return new string(code);
        }
        void InvokeRoomJoinedEvent(StartGameResult startGameResult)
        {
            if(startGameResult.Ok)
            {
                OnPlayerJoinedEvent?.Invoke(Runner, Runner.LocalPlayer);
            }
            else
            {
                OnJoinFailedEvent?.Invoke(startGameResult.ErrorMessage);
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            OnDisconnectedFromServerEvent?.Invoke(runner, reason);
        }

        #region Unused Callbacks
        public void OnConnectedToServer(NetworkRunner runner){}
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){}
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player){}
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        #endregion
    }
}