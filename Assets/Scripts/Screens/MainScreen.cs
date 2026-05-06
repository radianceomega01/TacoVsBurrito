using TacoVsBurrito;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene(NamingUtils._GameplayScreen);
    }
}
