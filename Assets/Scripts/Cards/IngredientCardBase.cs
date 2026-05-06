using System.Drawing;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class IngredientCardBase : CardBase
    {
        [Header("Scoring")]
        [SerializeField] int cardValue = 1;

        [Header("Fields")]
        [SerializeField] TextMeshProUGUI ValueTxtField;
        public int CardValue { get { return cardValue; } }

        protected override void Start()
        {
            base.Start();
            ValueTxtField.text = cardValue.ToString();
        }
        public override int GetModifiedMealScore(int currentScore)
        {
            return currentScore + cardValue;
        }

    }
}