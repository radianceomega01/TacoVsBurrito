using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TurnIndicatorUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI turnTxt;

        void OnEnable()
        {
            GameEvents.OnTurnStarted += UpdateTxt;
            GameEvents.OnCardSelectionForFoodFightWinner += UpdateTxtForFoodFightWinner;
        }

        void OnDisable()
        {
            GameEvents.OnTurnStarted -= UpdateTxt;
            GameEvents.OnCardSelectionForFoodFightWinner -= UpdateTxtForFoodFightWinner;
        }
        void Start()
        {
            turnTxt.text = "";
        }
        void UpdateTxt(PlayerBase player)
        {
            turnTxt.text = player is SelfPlayer ? "Your Turn!" : "Opponent's Turn!";
        }
        void UpdateTxtForFoodFightWinner(Dictionary<CardBase, int> dictionary, PlayerBase winner) => UpdateTxt(winner);

    }
}
