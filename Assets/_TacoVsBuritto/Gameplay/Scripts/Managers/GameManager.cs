// ============================================================
//  GameManager.cs  –  Core game controller
//
//  REAL RULES implemented minute-by-minute:
//
//  SETUP
//    - 2-4 playerBasePlayerBases (base), 2-8 with Foodie Expansion
//    - Each playerBasePlayerBase picks Taco or Burrito holder (cosmetic only)
//    - Deal 5 cards each; Health Inspectors reshuffled out
//    - Youngest playerBasePlayerBase goes first; play clockwise
//
//  TURN STRUCTURE (draw one, play one)
//    1. Draw one card from draw pile
//       - If Health Inspector: trigger immediately, turn ends
//       - If draw pile empty: skip draw step (no reshuffle of trash)
//    2. Play exactly ONE card from hand OR discard one card
//       a) Ingredient / Tummy Ache / Hot Sauce Boss → place in ANY meal
//       b) Action card → resolve effect, goes to Trash
//       c) Discard a card → goes to Trash, turn ends (counts as your play)
//    3. Before resolution of (a) or (b), any other playerBasePlayerBase may
//       interrupt with No Bueno (except Health Inspector / last card)
//
//  END OF GAME
//    - When draw pile is EMPTY: continue playing, skip draw step
//    - Game ends the INSTANT any playerBasePlayerBase plays their last card
//    - If that final card is an action, effect happens FIRST
//    - No Bueno CANNOT block the last card played
//
//  SCORING
//    - Sum of all Ingredients + Tummy Aches in your meal
//    - × 2 per Hot Sauce Boss (1 boss = ×2, 2 bosses = ×4)
//    - Highest score wins
//    - TIE: tied playerBasePlayerBases do a Food Fight tiebreaker
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] DrawPile drawPile;
        [SerializeField] TrashPile trashPile;
        [SerializeField] PlayArea playArea;

        const int FRAME_RATE = 90;

        // -------------------------------------------------------
        //  Singleton
        // -------------------------------------------------------
        public static GameManager Instance { get; private set; }

        // -------------------------------------------------------
        //  Runtime state
        // -------------------------------------------------------
        private List<PlayerBase> _players = new List<PlayerBase>();
        private ActionResolver _resolver;
        private TurnHandler _turnHandler;
        private GameState gameState = GameState.None;

        // -------------------------------------------------------
        //  Public accessors
        // -------------------------------------------------------
        public IReadOnlyList<PlayerBase> Players => _players;
        public PlayerBase CurrentPlayer => _turnHandler.CurrentPlayer;

        // -------------------------------------------------------

        private GameManager() { }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            _resolver = new ActionResolver(drawPile, trashPile, _players);
            _turnHandler = new TurnHandler(trashPile);

            GameEvents.OnGameOver += ManageGameOver;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameOver -= ManageGameOver;
        }

        void Start()
        {
            Application.targetFrameRate = FRAME_RATE;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // -------------------------------------------------------
        //  Game Start
        // -------------------------------------------------------

        public void AddPlayerBeforeGameStarts(PlayerBase player)
        {
            _players.Add(player);
            GameEvents.OnActionResolverSet?.Invoke(_resolver);
        }

        public void CardDrawnFromPile(CardBase card)
        {

            // Health Inspector triggers IMMEDIATELY on draw – unblockable
            if (card is HealthInspectorCard)
            {
                playArea.PlayAction(card);
                return;
            }
            else
            {
                if(CurrentPlayer is SelfPlayer)
                {
                    GameEvents.OnLogMessage?.Invoke("Play a Card!");
                }
            }

            // Normal card: add to hand
            CurrentPlayer.Hand.AddCard(card);
            GameEvents.OnCardDrawn?.Invoke(CurrentPlayer, card);
            GameEvents.OnHandChanged?.Invoke(CurrentPlayer);
            _turnHandler.GoToNextState();
        }

        // -------------------------------------------------------
        //  End of game
        // -------------------------------------------------------

        private IEnumerator FinalScoring()
        {
            yield return new WaitForSeconds(0.5f);

            // Sort playerBasePlayerBases by score descending
            var ranked = _players.OrderByDescending(p => p.Score).ToList();

            GameEvents.OnLogMessage?.Invoke("\n🏆 FINAL SCORES:");
            foreach (var p in ranked)
                GameEvents.OnLogMessage?.Invoke($"  {p.Name}: {p.Score} pts");

            // Check for tie among the top scorers
            int topScore = ranked[0].Score;
            var tiedWinners = ranked.Where(p => p.Score == topScore).ToList();

            if (tiedWinners.Count == 1)
            {
                GameEvents.OnLogMessage?.Invoke($"\n🎉 WINNER: {ranked[0].Name} with {topScore} pts!");
                GameEvents.OnGameOver?.Invoke(ranked[0]);
            }
            else
            {
                // Tiebreaker: Food Fight between tied playerBasePlayerBases
                GameEvents.OnLogMessage?.Invoke(
                    $"\n🤝 TIE between: {string.Join(", ", tiedWinners.Select(p => p.Name))}!");
                GameEvents.OnLogMessage?.Invoke("🍽 FOOD FIGHT TIEBREAKER!");

                yield return StartCoroutine(FoodFightTiebreaker(tiedWinners));
            }
        }

        private IEnumerator FoodFightTiebreaker(List<PlayerBase> tiedPlayerBases)
        {
            // Each tied playerBasePlayerBase flips one card; highest ingredient wins
            var flips = new Dictionary<PlayerBase, CardBase>();
            foreach (var p in tiedPlayerBases)
            {
                var flipped = drawPile.FlipTop();
                if (flipped != null)
                {
                    flips[p] = flipped;
                    GameEvents.OnLogMessage?.Invoke(
                        $"  {p.Name} flips: {flipped.Name} [{(flipped is IngredientCardBase @base ? @base.CardValue + "pt" : "0pt")}]");
                }
            }

            if (flips.Count == 0) { GameEvents.OnGameOver?.Invoke(tiedPlayerBases[0]); yield break; }

            int best = flips.Values.Max(c => c is IngredientCardBase @base ? @base.CardValue : 0);
            var winner = flips.FirstOrDefault(kv =>
                (kv.Value is IngredientCardBase @base ? @base.CardValue : 0) == best).Key;

            // Shuffle flipped cards back
            drawPile.ShuffleBackIn(flips.Values.ToList());

            GameEvents.OnLogMessage?.Invoke($"\n🎉 TIEBREAKER WINNER: {winner?.Name ?? tiedPlayerBases[0].Name}!");
            GameEvents.OnGameOver?.Invoke(winner ?? tiedPlayerBases[0]);
        }

        void ManageGameOver(PlayerBase finishingPlayer)
        {
            Tuple<PlayerBase, int> winnerWithScore = Tuple.Create<PlayerBase, int>(null, 0);
            foreach(PlayerBase player in _players)
            {
                if(player.Score > winnerWithScore.Item2)
                {
                    winnerWithScore = Tuple.Create(player, player.Score);
                }
            }
            GameEvents.OnLogMessage?.Invoke("Player: "+winnerWithScore.Item1+ " won the game with "+ winnerWithScore.Item2 + " score!");
        }

        private List<PlayerBase> GetClockwiseOrderFrom(PlayerBase start)
        {
            var order = new List<PlayerBase>();
            int idx = _players.IndexOf(start);
            for (int i = 0; i < _players.Count; i++)
                order.Add(_players[(idx + i) % _players.Count]);
            return order;
        }
        
        public TrashPile GetTrashPile() => trashPile;
        public DrawPile GetDrawPile() => drawPile;
        public PlayArea GetPlayArea() => playArea;
    }
    public enum GameState
    {
        None,
        Init,
        CardShuffle,
        CardDistribution,
        Running,
        Completed

    }
}