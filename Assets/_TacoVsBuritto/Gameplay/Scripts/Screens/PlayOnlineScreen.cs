using System;
using System.Threading.Tasks;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TacoVsBurrito
{
    public class PlayOnlineScreen : MonoBehaviour
    {
        [SerializeField] GameObject modeSelectionPanel;
        [SerializeField] GameObject playerSelectionPanel;
        [SerializeField] GameObject createAndJoinPanel;
        [SerializeField] RoomPlayersPanel roomPlayersPanel;
        [SerializeField] TMP_InputField nameInputField;
        [SerializeField] Slider progressBar;

        const float PROGRESS_TIME = 3f;
        JoinRoomType joinType = JoinRoomType.None;

        void OnEnable()
        {
            FusionNetworkManager.Instance.OnPlayerJoinedEvent += ManagePlayerJoined;
        }
        void OnDisable()
        {
            FusionNetworkManager.Instance.OnPlayerJoinedEvent -= ManagePlayerJoined;
        }

        void ManagePlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            EndProgressBar();
            switch(joinType)
            {
                case JoinRoomType.CreateRoom:
                case JoinRoomType.JoinRoom:
                    createAndJoinPanel.SetActive(false);
                    roomPlayersPanel.gameObject.SetActive(true);
                    roomPlayersPanel.InitWithRoomName(runner.SessionInfo.Name);
                    roomPlayersPanel.AddPlayer(player.PlayerId.ToString());
                    break;
            }
        }

        public async void OnPlayWithFriendsClicked()
        {
            modeSelectionPanel.SetActive(false);
            createAndJoinPanel.SetActive(true);
        }
        public async void OnCreateRoomClicked()
        {
            BeginProgressBar();
            joinType = JoinRoomType.CreateRoom;
            FusionNetworkManager.Instance.CreateFriendRoom(4);
        }
        public async void OnJoinRoomClicked()
        {
            BeginProgressBar();
            joinType = JoinRoomType.JoinRoom;
            FusionNetworkManager.Instance.CreateFriendRoom(4);
        }
        public async void OnPlayRandomClicked()
        {
            //BeginProgressBar();
        }

        void BeginProgressBar()
        {
            progressBar.DOValue(1f, PROGRESS_TIME);
        }
        void EndProgressBar()
        {
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
