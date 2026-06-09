using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TacoVsBurrito
{
    public class GameoverPanelUI : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] TextMeshProUGUI panelTxt;

        void OnEnable()
        {
            GameEvents.OnGameWinner += ManageGameWinner;
        }
        void OnDisable()
        {
            GameEvents.OnGameWinner -= ManageGameWinner;
        }

        private void ManageGameWinner(Tuple<PlayerBase, int> winnerWithScore)
        {
            container.gameObject.SetActive(true);
            panelTxt.SetText($"Player {winnerWithScore.Item1.Name} won the game with score {winnerWithScore.Item2}!");
        }
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
