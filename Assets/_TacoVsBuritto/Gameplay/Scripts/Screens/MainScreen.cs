using TacoVsBurrito;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    public void startGame()
    {
        GameEvents.OnGUIClickSFX?.Invoke();
        SceneManager.LoadScene(NamingUtils._GameplayScreen);
    }
}
