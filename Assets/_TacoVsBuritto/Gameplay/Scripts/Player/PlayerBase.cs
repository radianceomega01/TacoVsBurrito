using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class PlayerBase : MonoBehaviour
    {

        private PlayerHand hand;
        private Meal meal;
        FullScalePlayerView fullScaleView;
        MinifiedPlayerView minifiedView;
        public int Index { get; private set; }
        public string Name { get; private set; }

        public PlayerHand Hand { get { return hand; } }
        public Meal Meal { get { return meal; } }

        // Turn-state flags
        public bool SkipNextTurn { get; set; } = false;   // (not in base rules but useful for expansions)
        public bool HasNoBuenoReady { get; set; } = false;   // UI hint only

        // Trash Panda usage counter (max 2 Trash Pandas retrieved per game per official FAQ)
        //public int TrashPandaRetrievedCount { get; set; } = 0;

        protected virtual void Awake()
        {
            hand = GetComponentInChildren<PlayerHand>();
            meal = GetComponentInChildren<Meal>();
            fullScaleView = GetComponentInChildren<FullScalePlayerView>();
            minifiedView = GetComponentInChildren<MinifiedPlayerView>(true);

            GameEvents.OnFoodFightAction += ManagePlayerOnFoodFight;
            GameEvents.OnFoodFightOver += ManagePlayerOnFoodFightOver;
        }
        protected virtual void OnDestroy() 
        {
            GameEvents.OnFoodFightAction -= ManagePlayerOnFoodFight;
            GameEvents.OnFoodFightOver -= ManagePlayerOnFoodFightOver;
        }

        public void InitIndex(int index)
        {
            Index = index;
        }

        public int Score => Meal.CalculateScore();

        protected void ManagePlayerOnFoodFightOver()
        {
            fullScaleView.gameObject.SetActive(true);
            minifiedView.DisableView();
        }

        protected void ManagePlayerOnFoodFight(FoodFightCard card)
        {
            fullScaleView.gameObject.SetActive(false);
            minifiedView.EnableView(Score, hand.Count);
        }

    }
}