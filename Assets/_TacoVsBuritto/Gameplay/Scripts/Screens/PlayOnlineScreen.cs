using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

namespace TacoVsBurrito
{
    public class PlayOnlineScreen : MonoBehaviour
    {
        [SerializeField] RoomManager roomManager;
        [SerializeField] GameDataSO gameDataSO;
        [SerializeField] GameObject modeSelectionPanel;
        [SerializeField] GameObject playerSelectionPanel;
        [SerializeField] GameObject createAndJoinPanel;
        [SerializeField] RoomPlayersPanel roomPlayersPanel;
        [SerializeField] TMP_InputField nameInputField;
        [SerializeField] TMP_InputField roomNameInputField;
        [SerializeField] Slider progressBar;
        [SerializeField] TextMeshProUGUI errorMsg;

        const float PROGRESS_TIME_IN_SECS = 3f;
        const int ERRRO_MSG_DURATION_IN_MS = 2000;
        JoinRoomType joinType = JoinRoomType.None;

        void Start()
        {
            FusionNetworkManager.Instance.Init();

            FusionNetworkManager.Instance.OnLocalPlayerJoinedEvent += ManageLocalPlayerJoined;
            FusionNetworkManager.Instance.OnJoinLobbyFailedEvent += ManageJoinLobbyFailed;
            FusionNetworkManager.Instance.OnJoinFailedEvent += ManageJoinFailed;
            FusionNetworkManager.Instance.OnSceneLoadDoneEvent += ManageSceneLoadDone;

            roomManager.OnRoomPlayersUpdated += ManageRoomPlayersUpdated;
        }

        void OnDestroy()
        {
            FusionNetworkManager.Instance.OnLocalPlayerJoinedEvent -= ManageLocalPlayerJoined;
            FusionNetworkManager.Instance.OnJoinLobbyFailedEvent -= ManageJoinLobbyFailed;
            FusionNetworkManager.Instance.OnJoinFailedEvent -= ManageJoinFailed;
            FusionNetworkManager.Instance.OnSceneLoadDoneEvent -= ManageSceneLoadDone;

            roomManager.OnRoomPlayersUpdated -= ManageRoomPlayersUpdated;
        }

        private void ManageLocalPlayerJoined(NetworkRunner runner)
        {
            EndProgressBar();
            switch (joinType)
            {
                case JoinRoomType.CreateRoom:
                    playerSelectionPanel.SetActive(false);
                    break;
                case JoinRoomType.JoinRoom:
                    createAndJoinPanel.SetActive(false);
                    break;
            }
            roomPlayersPanel.gameObject.SetActive(true);
            roomPlayersPanel.InitWithRoomName(runner.SessionInfo.Name);
            SendPlayerNameRPC();
        }

        private async void SendPlayerNameRPC()
        {
            while(!roomManager.IsSpawned)
            {
                await Task.Yield();
            }
            roomManager.RPC_SetPlayerName(nameInputField.text);
        }

        private void ManageRoomPlayersUpdated(List<PlayerData> players)
        {
            Debug.LogWarning("reached ManageRoomPlayersUpdated");
            if(roomPlayersPanel.NumOfPlayers == players.Count)
            {
                roomPlayersPanel.UpdatePlayerNames(players);
            }
            else if(roomPlayersPanel.NumOfPlayers < players.Count)
            {
                roomPlayersPanel.AddPlayer(players[^1]);
            }
            else
            {
                //roomPlayersPanel.RemovePlayer(players);
            }
            
        }

        private void ManageSceneLoadDone(NetworkRunner runner)
        {
            if(runner.IsServer)
            {
                runner.Spawn(roomManager, Vector3.zero, Quaternion.identity);
            }
        }

        void ManageJoinFailed(string msg)
        {
            ShowErrorMsg(msg);
        }

        void ManageJoinLobbyFailed(NetworkRunner runner, string msg)
        {
            ShowErrorMsg(msg);
        }

        public async void OnPlayWithFriendsClicked()
        {
            if (ValidatePlayerName())
            {
                modeSelectionPanel.SetActive(false);
                createAndJoinPanel.SetActive(true);
            }
        }
        public async void OnCreateRoomClicked()
        {
            createAndJoinPanel.SetActive(false);
            playerSelectionPanel.SetActive(true);
        }

        public void OnPlay2pClicked()
        {
            BeginProgressBar();
            gameDataSO.numOfPlayers = 2;
            joinType = JoinRoomType.CreateRoom;
            FusionNetworkManager.Instance.CreateFriendRoom(gameDataSO.numOfPlayers);
        }
        public void OnPlay3pClicked()
        {
            BeginProgressBar();
            gameDataSO.numOfPlayers = 3;
            joinType = JoinRoomType.CreateRoom;
            FusionNetworkManager.Instance.CreateFriendRoom(gameDataSO.numOfPlayers);
        }
        public void OnPlay4pClicked()
        {
            BeginProgressBar();
            gameDataSO.numOfPlayers = 4;
            joinType = JoinRoomType.CreateRoom;
            FusionNetworkManager.Instance.CreateFriendRoom(gameDataSO.numOfPlayers);
        }

        public async void OnJoinRoomClicked()
        {
            if (ValidateRoomName())
            {
                BeginProgressBar();
                joinType = JoinRoomType.JoinRoom;
                FusionNetworkManager.Instance.JoinFriendRoom(roomNameInputField.text.ToUpper());
            }
        }

        public async void OnBackBtnClicked()
        {
            await FusionNetworkManager.Instance.LeaveRoom();
            SceneManager.LoadScene(NamingUtils._MainScreen);
        }

        public async void OnPlayRandomClicked()
        {
            //Todo: Implement later
            //BeginProgressBar();
        }

        bool ValidatePlayerName()
        {
            if (nameInputField.text.IsNullOrEmpty())
            {
                ShowErrorMsg("Name cannot be empty!");
                return false;
            }
            return true;
        }
        bool ValidateRoomName()
        {
            if (roomNameInputField.text.IsNullOrEmpty())
            {
                ShowErrorMsg("Room Name cannot be empty!");
                return false;
            }
            return true;
        }
        async void ShowErrorMsg(String msg)
        {
            errorMsg.SetText(msg);
            await Task.Delay(ERRRO_MSG_DURATION_IN_MS);
            errorMsg.SetText(String.Empty);
        }

        void BeginProgressBar()
        {
            progressBar.DOValue(1f, PROGRESS_TIME_IN_SECS);
        }
        void EndProgressBar()
        {
            if (progressBar.value == 0f)
                return;
            progressBar.DOKill();
            progressBar.value = 0f;
        }
    }

    public enum JoinRoomType
    {
        None,
        CreateRoom,
        JoinRoom,
        PlayRandom
    }
}
