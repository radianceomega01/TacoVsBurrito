using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;

namespace TacoVsBurrito
{
    public class MainScreen : MonoBehaviour
    {
        [SerializeField] Slider progressBar;
        const float PROGRESS_TIME = 3f;

        void Start()
        {
            FusionNetworkManager.Instance.OnConnectedToServerEvent += ManageOnConnectedToServerEvent;
            FusionNetworkManager.Instance.OnConnectFailedEvent += ManageOnConnectFailed;
        }

        void OnDestroy()
        {
            FusionNetworkManager.Instance.OnConnectedToServerEvent -= ManageOnConnectedToServerEvent;
            FusionNetworkManager.Instance.OnConnectFailedEvent -= ManageOnConnectFailed;
        }

        private void ManageOnConnectedToServerEvent(NetworkRunner runner)
        {
            progressBar.value = 0;
            SceneManager.LoadScene(NamingUtils._PlayOnlineScreen);
        }

        private void ManageOnConnectFailed(NetworkRunner runner, string reason)
        {
            EndProgressBar();
            Debug.LogWarning(reason.ToString());
        }

        public async void PlayOnline()
        {
            GameEvents.OnGUIClickSFX?.Invoke();
            BeginProgressBar();
            await FusionNetworkManager.Instance.Init();
            
        }
        public void PlayOffline()
        {
            GameEvents.OnGUIClickSFX?.Invoke();
            SceneManager.LoadScene(NamingUtils._PlayOfflineScreen);
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
}
