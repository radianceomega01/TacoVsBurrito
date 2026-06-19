using System;
using TacoVsBurrito;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TacoVsBurrito
{
    public class PlayOfflineScreen : MonoBehaviour
    {
        [SerializeField] GameDataSO gameData;

        public void OnPlay2P()
        {
            gameData.numOfPlayers = 2;
            startGame();
        }
        public void OnPlay3P()
        {
            gameData.numOfPlayers = 3;
            startGame();
        }
        public void OnPlay4P()
        {
            gameData.numOfPlayers = 4;
            startGame();
        }
        void startGame()
        {
            GameEvents.OnGUIClickSFX?.Invoke();
            SceneManager.LoadScene(NamingUtils._GameplayScreen);
        }
    }
}
