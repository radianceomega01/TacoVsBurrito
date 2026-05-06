using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class PlayerBase: MonoBehaviour
    {
        public int        Index      { get; private set; }
        public string     Name       { get; private set; }
        public MealType   MealChoice { get; private set; }
        public PlayerType Type       { get; private set; }

        public Hand Hand { get; private set; } = new Hand();
        public Meal Meal { get; private set; }

        // Turn-state flags
        public bool SkipNextTurn    { get; set; } = false;   // (not in base rules but useful for expansions)
        public bool HasNoBuenoReady { get; set; } = false;   // UI hint only

        // Trash Panda usage counter (max 2 Trash Pandas retrieved per game per official FAQ)
        //public int TrashPandaRetrievedCount { get; set; } = 0;

        public void InitPlayer(int index, string name, MealType meal)
        {
            Index      = index;
            Name       = name;
            MealChoice = meal;
            Meal       = new Meal(meal);
        }

        public int Score => Meal.CalculateScore();

        /// Swap HAND and MEAL with another player (Order Envy).
        public void SwapMeal(PlayerBase other)
        {
            // Swap meal contents (we keep the meal HOLDER type; only contents swap)
            var myMealCards    = Meal.TakeAll();
            var theirMealCards = other.Meal.TakeAll();
            foreach (var c in theirMealCards) Meal.AddCard(c);
            foreach (var c in myMealCards)    other.Meal.AddCard(c);
        }

        public override string ToString() =>
            $"Player {Index}: {Name} | {MealChoice} | Score: {Score} | Hand: {Hand.Count}";
  
    }
    public enum PlayerType
    {
        Human,
        AI
    }

}