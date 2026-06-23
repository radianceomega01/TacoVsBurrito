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
        public async void PlayOnline()
        {
            GameEvents.OnGUIClickSFX?.Invoke();
            SceneManager.LoadScene(NamingUtils._PlayOnlineScreen);
            
        }
        public void PlayOffline()
        {
            GameEvents.OnGUIClickSFX?.Invoke();
            SceneManager.LoadScene(NamingUtils._PlayOfflineScreen);
        }
    }
}
