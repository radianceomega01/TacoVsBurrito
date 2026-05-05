namespace TacoVsBurrito
{
    public class Player
    {
        public int        Index      { get; }
        public string     Name       { get; set; }
        public MealType   MealChoice { get; set; }
        public PlayerType Type       { get; set; }

        public Hand Hand { get; private set; } = new Hand();
        public Meal Meal { get; private set; }

        // Turn-state flags
        public bool SkipNextTurn    { get; set; } = false;   // (not in base rules but useful for expansions)
        public bool HasNoBuenoReady { get; set; } = false;   // UI hint only

        // Trash Panda usage counter (max 2 Trash Pandas retrieved per game per official FAQ)
        public int TrashPandaRetrievedCount { get; set; } = 0;

        public Player(int index, string name, MealType meal, PlayerType type = PlayerType.Human)
        {
            Index      = index;
            Name       = name;
            MealChoice = meal;
            Type       = type;
            Meal       = new Meal(meal);
        }

        public int Score => Meal.CalculateScore();

        /// Swap HAND and MEAL with another player (Order Envy).
        public void SwapHandAndMeal(Player other)
        {
            // Swap hands
            var myCards    = Hand.TakeAll();
            var theirCards = other.Hand.TakeAll();
            Hand.ReplaceWith(theirCards);
            other.Hand.ReplaceWith(myCards);

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