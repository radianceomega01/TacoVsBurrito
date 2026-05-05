using System.Drawing;
using UnityEngine;

namespace TacoVsBurrito
{
    public class IngredientCardBase : CardBase
    {
        [Header("Scoring")]
        [SerializeField] int cardValue = 1;
        public int CardValue {get{ return cardValue; } }

        public override int GetModifiedMealScore(int currentScore)
        {
            return currentScore + cardValue;
        }

    }
}