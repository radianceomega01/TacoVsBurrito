using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class UiTurnIndicator : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI turnTxt;

        void Awake()
        {
            GameEvents.OnTurnStarted += UpdateTxt;
            GameEvents.OnCardSelectionForFoodFightWinner += UpdateTxtForFoodFightWinner;
        }

        void OnDestroy()
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
