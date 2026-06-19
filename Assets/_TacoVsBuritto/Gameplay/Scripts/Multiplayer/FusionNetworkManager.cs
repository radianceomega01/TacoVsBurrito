using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TacoVsBurrito
{
    public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static FusionNetworkManager Instance;

        private NetworkRunner Runner;

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
        }

        private async Task EnsureRunner()
        {
            if (Runner != null)
                return;

            Runner = gameObject.AddComponent<NetworkRunner>();
            Runner.ProvideInput = true;
        }

        public async Task CreateFriendRoom(string roomCode)
        {
            await EnsureRunner();

            var result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = roomCode,
                PlayerCount = 8
            });

            Debug.Log($"Host Result: {result.Ok}");
        }

        public async Task JoinFriendRoom(string roomCode)
        {
            await EnsureRunner();

            var result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = roomCode
            });

            Debug.Log($"Join Result: {result.Ok}");
        }

        public async Task JoinLobby()
        {
            await EnsureRunner();

            await Runner.JoinSessionLobby(SessionLobby.ClientServer);
        }

        public async Task JoinOnlineGame(string sessionName)
        {
            await EnsureRunner();

            await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = sessionName
            });
        }

        public List<SessionInfo> AvailableSessions = new();

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

        #region Unused Callbacks

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
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