using System;
using TacoVsBurrito;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    public void PlayOnline()
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
