using System;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class GameoverPanelUI : MonoBehaviour
    {
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
            panelTxt.SetText($"Player {winnerWithScore.Item1} won the game with score {winnerWithScore.Item2}!");
        }
    }
}
