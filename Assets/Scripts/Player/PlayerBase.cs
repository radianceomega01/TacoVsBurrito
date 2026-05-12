using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class PlayerBase : MonoBehaviour
    {
        private Hand hand;
        private Meal meal;
        public int Index { get; private set; }
        public string Name { get; private set; }

        public Hand Hand { get { return hand; } }
        public Meal Meal { get { return meal; } }

        // Turn-state flags
        public bool SkipNextTurn { get; set; } = false;   // (not in base rules but useful for expansions)
        public bool HasNoBuenoReady { get; set; } = false;   // UI hint only

        // Trash Panda usage counter (max 2 Trash Pandas retrieved per game per official FAQ)
        //public int TrashPandaRetrievedCount { get; set; } = 0;

        protected virtual void Awake()
        {
            hand = GetComponentInChildren<Hand>();
            meal = GetComponentInChildren<Meal>();
        }
        public void InitPlayer(int index, string name, MealType meal)
        {
            Index = index;
            Name = name;
        }

        public int Score => Meal.CalculateScore();

        public void SwapMeal(PlayerBase other)
        {
            // Cache original meal parents
            Transform myMealParent = meal.ParentTransform;
            Transform otherMealParent = other.meal.ParentTransform;

            // Swap meal visuals/parents
            meal.ChangeParentAndPosition(otherMealParent, other.meal.transform.position);
            other.meal.ChangeParentAndPosition(myMealParent, meal.transform.position);

            // Swap references
            Meal tempMeal = meal;
            meal = other.meal;
            other.meal = tempMeal;

            // Cache original hand parents
            Transform myHandParent = hand.ParentTransform;
            Transform otherHandParent = other.hand.ParentTransform;

            // Swap hand visuals/parents
            hand.ChangeParentAndPosition(otherHandParent, other.hand.transform.position);
            other.hand.ChangeParentAndPosition(myHandParent, hand.transform.position);

            // Swap references
            Hand tempHand = hand;
            hand = other.hand;
            other.hand = tempHand;
        }

        public override string ToString() =>
            $"Player {Index}: {Name} | Score: {Score} | Hand: {Hand.Count}";

    }
}