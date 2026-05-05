// ============================================================
//  GameManager.cs  –  Core game controller
//
//  REAL RULES implemented minute-by-minute:
//
//  SETUP
//    - 2-4 players (base), 2-8 with Foodie Expansion
//    - Each player picks Taco or Burrito holder (cosmetic only)
//    - Deal 5 cards each; Health Inspectors reshuffled out
//    - Youngest player goes first; play clockwise
//
//  TURN STRUCTURE (draw one, play one)
//    1. Draw one card from draw pile
//       - If Health Inspector: trigger immediately, turn ends
//       - If draw pile empty: skip draw step (no reshuffle of trash)
//    2. Play exactly ONE card from hand OR discard one card
//       a) Ingredient / Tummy Ache / Hot Sauce Boss → place in ANY meal
//       b) Action card → resolve effect, goes to Trash
//       c) Discard a card → goes to Trash, turn ends (counts as your play)
//    3. Before resolution of (a) or (b), any other player may
//       interrupt with No Bueno (except Health Inspector / last card)
//
//  END OF GAME
//    - When draw pile is EMPTY: continue playing, skip draw step
//    - Game ends the INSTANT any player plays their last card
//    - If that final card is an action, effect happens FIRST
//    - No Bueno CANNOT block the last card played
//
//  SCORING
//    - Sum of all Ingredients + Tummy Aches in your meal
//    - × 2 per Hot Sauce Boss (1 boss = ×2, 2 bosses = ×4)
//    - Highest score wins
//    - TIE: tied players do a Food Fight tiebreaker
// ============================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TacoVsBurrito
{
    public class GameManager : MonoBehaviour
    {
        // -------------------------------------------------------
        //  Inspector fields
        // -------------------------------------------------------

        [Header("Game Settings")]
        [Range(2, 8)] public int numberOfPlayers = 4;
        private const int STARTING_HAND_SIZE = 5;

        // -------------------------------------------------------
        //  Singleton
        // -------------------------------------------------------
        public static GameManager Instance { get; private set; }

        // -------------------------------------------------------
        //  Runtime state
        // -------------------------------------------------------
        private List<Player>   _players        = new List<Player>();
        private DeckManager    _deck           = new DeckManager();
        private ActionResolver _resolver;

        private int  _currentIndex  = 0;
        private bool _gameRunning   = false;
        private bool _isDrawPileGone  = false;        // once empty, skip draw step

        // Human-turn gate
        private bool _humanPlayedCard = false;

        // No Bueno interrupt gate
        private bool _noBuenoWindowOpen   = false;
        private bool _noBuenoWasPlayed    = false;
        private Player _noBuenoBlocker    = null;
        private NoBuenoCard   _noBuenoCard       = null;

        // -------------------------------------------------------
        //  Public accessors
        // -------------------------------------------------------
        public IReadOnlyList<Player> Players     => _players;
        public Player CurrentPlayer              => _players[_currentIndex];
        public bool   IsDrawPileEmpty              => _isDrawPileGone;
        public DeckManager Deck => _deck;

        // -------------------------------------------------------

        private GameManager() { }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _resolver = new ActionResolver(_deck, () => _players);
        }

        private void OnDestroy() => GameEvents.Clear();

        // -------------------------------------------------------
        //  Game Start
        // -------------------------------------------------------
        public void StartGame(List<PlayerSetupData> setupData)
        {
            int count = setupData.Count;
            if (count < 2 || count > 8)
            {
                Debug.LogError("[GameManager] Player count must be 2–8.");
                return;
            }

            _players.Clear();

            for (int i = 0; i < count; i++)
            {
                var d = setupData[i];
                _players.Add(new Player(i, d.playerName, d.mealType, d.playerType));
            }

            // Deal starting hands (Health Inspectors filtered out)
            foreach (var p in _players)
            {
                var hand = _deck.DealStartingHand(STARTING_HAND_SIZE);
                foreach (var c in hand) p.Hand.AddCard(c);
                GameEvents.OnHandChanged?.Invoke(p);
            }

            _currentIndex  = 0;
            _gameRunning   = true;
            _isDrawPileGone  = false;

            GameEvents.OnGameStarted?.Invoke(_players);
            GameEvents.OnLogMessage?.Invoke(
                $"🎮 Game started! {count} players. Youngest goes first. Play clockwise.");

            StartCoroutine(GameLoop());
        }

        // -------------------------------------------------------
        //  Main game loop
        // -------------------------------------------------------
        private IEnumerator GameLoop()
        {
            while (_gameRunning)
            {
                var current = CurrentPlayer;
                GameEvents.OnTurnStarted?.Invoke(current);
                GameEvents.OnLogMessage?.Invoke($"\n--- {current.Name}'s turn ---");

                // ---- 1. DRAW PHASE ----
                if (!_isDrawPileGone)
                {
                    CardBase drawn = _deck.Draw();

                    if (drawn == null)
                    {
                        // Draw pile just became empty
                        _isDrawPileGone = true;
                        GameEvents.OnDrawPileEmpty?.Invoke();
                        GameEvents.OnLogMessage?.Invoke(
                            "📭 Draw pile is empty! Players now skip the draw step.");
                    }
                    else
                    {
                        GameEvents.OnLogMessage?.Invoke($"  {current.Name} draws a card.");

                        // Health Inspector triggers IMMEDIATELY on draw – unblockable
                        if (drawn is HealthInspectorCard @base)
                        {
                            string hiLog = _resolver.TriggerHealthInspector(current, @base);
                            GameEvents.OnLogMessage?.Invoke(hiLog);
                            GameEvents.OnHealthInspector?.Invoke(current);
                            GameEvents.OnTurnEnded?.Invoke(current);
                            AdvanceToNextPlayer();
                            yield return new WaitForSeconds(1.5f);
                            continue;                // skip play phase – turn is over
                        }

                        // Normal card: add to hand
                        current.Hand.AddCard(drawn);
                        GameEvents.OnCardDrawn?.Invoke(current, drawn);
                        GameEvents.OnHandChanged?.Invoke(current);
                    }
                }
                else
                {
                    GameEvents.OnDrawPhaseSkipped?.Invoke(true);
                    GameEvents.OnLogMessage?.Invoke($"  (Draw pile empty – skip draw step)");
                }

                // ---- 2. PLAY PHASE ----
                // Check if this player is out of cards (game ends if so)
                if (current.Hand.Count == 0)
                {
                    // Should not normally happen mid-loop but guard it
                    GameEvents.OnLogMessage?.Invoke($"  {current.Name} has no cards – skipping play.");
                    GameEvents.OnTurnEnded?.Invoke(current);
                    AdvanceToNextPlayer();
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                if (current.Type == PlayerType.AI)
                {
                    yield return StartCoroutine(ExecuteAITurn(current));
                }
                else
                {
                    // Human: wait until PlayCard() or DiscardCard() is called
                    _humanPlayedCard = false;
                    yield return new WaitUntil(() => _humanPlayedCard || !_gameRunning);
                }

                if (!_gameRunning) yield break;

                GameEvents.OnTurnEnded?.Invoke(current);
                AdvanceToNextPlayer();
                yield return new WaitForSeconds(0.25f);
            }
        }

        // -------------------------------------------------------
        //  Human API  (called by UI)
        // -------------------------------------------------------

        /// Human places an ingredient/tummy ache/hot sauce into a meal.
        /// destPlayerIndex: whose meal to place it in (-1 = own meal).
        public void HumanPlacesCardInMeal(int handIndex, int destPlayerIndex)
        {
            if (!IsHumanTurn()) return;
            var current = CurrentPlayer;
            var card    = current.Hand.GetAt(handIndex);
            if (card == null || !card.IsPlaceableInMeal) return;

            var dest = destPlayerIndex < 0 ? current : GetPlayer(destPlayerIndex);
            if (dest == null) return;

            bool isLastCard = current.Hand.Count == 1;

            // Open No Bueno interrupt window (other players can react)
            StartCoroutine(NoBuenoInterruptWindow(current, card, isLastCard, () =>
            {
                if (_noBuenoWasPlayed) return; // blocked – card already trashed in NoBueno resolver

                string log = _resolver.PlaceCardInMeal(current, card, dest);
                GameEvents.OnLogMessage?.Invoke(log);
                GameEvents.OnMealScoreChanged?.Invoke(dest);
                GameEvents.OnScoreChanged?.Invoke(dest, dest.Score);

                if (isLastCard) TriggerGameEnd();
                else            _humanPlayedCard = true;
            }));
        }

        /// Human plays an action card.
        /// targetPlayerIndex: chosen target player (-1 if not needed yet).
        public void HumanPlaysActionCard(int handIndex, int targetPlayerIndex = -1)
        {
            if (!IsHumanTurn()) return;
            var current = CurrentPlayer;
            var card    = current.Hand.GetAt(handIndex);
            if (card == null) return;

            // Health Inspector can't be played from hand (only triggered on draw)
            if (card is HealthInspectorCard)
            {
                GameEvents.OnLogMessage?.Invoke("⚠ Health Inspector cannot be played from hand.");
                return;
            }

            bool isLastCard = current.Hand.Count == 1;

            StartCoroutine(NoBuenoInterruptWindow(current, card, isLastCard, () =>
            {
                if (_noBuenoWasPlayed) return;
                current.Hand.RemoveCard(card);
                StartCoroutine(ResolveActionCard(current, card, targetPlayerIndex, isLastCard));
            }));
        }

        /// Human discards a card (counts as their play for the turn).
        public void HumanDiscardsCard(int handIndex)
        {
            if (!IsHumanTurn()) return;
            var current = CurrentPlayer;
            var card    = current.Hand.GetAt(handIndex);
            if (card == null) return;

            bool isLastCard = current.Hand.Count == 1;
            current.Hand.RemoveCard(card);
            _deck.Trash(card);
            GameEvents.OnHandChanged?.Invoke(current);
            GameEvents.OnLogMessage?.Invoke($"  {current.Name} discarded '{card.Name}'.");

            if (isLastCard) TriggerGameEnd();
            else            _humanPlayedCard = true;
        }

        /// Human plays No Bueno to block a card mid-interrupt window.
        public void HumanPlaysNoBueno(int handIndex)
        {
            if (!_noBuenoWindowOpen) return;
            var current = CurrentPlayer; // the blocker can be ANY player (human only here)
            // Find the human player who has this No Bueno
            // For simplicity we allow any human player to trigger this
            foreach (var p in _players)
            {
                if (p.Type == PlayerType.Human)
                {
                    var nb = p.Hand.GetAt(handIndex);
                    if (nb != null && nb is NoBuenoCard)
                    {
                        _noBuenoBlocker  = p;
                        _noBuenoCard     = (NoBuenoCard)nb;
                        _noBuenoWasPlayed = true;
                        _noBuenoWindowOpen = false;
                        return;
                    }
                }
            }
        }

        // -------------------------------------------------------
        //  No Bueno interrupt window coroutine
        // -------------------------------------------------------
        private IEnumerator NoBuenoInterruptWindow(Player active, CardBase card,
                                                    bool isLastCard, System.Action onProceed)
        {
            _noBuenoWasPlayed  = false;
            _noBuenoBlocker    = null;
            _noBuenoCard       = null;

            // Health Inspector and last-card plays skip the window entirely
            if (card is HealthInspectorCard || isLastCard)
            {
                onProceed?.Invoke();
                yield break;
            }

            // Notify UI that a card is about to be played
            _noBuenoWindowOpen = true;
            GameEvents.OnCardAboutToBePlayed?.Invoke(active, card, isLastCard,
                (blocker, noBueno) => {
                    _noBuenoBlocker   = blocker;
                    _noBuenoCard      = (NoBuenoCard)noBueno;
                    _noBuenoWasPlayed = true;
                    _noBuenoWindowOpen = false;
                });

            // Wait a short window for human/AI No Bueno reactions
            float elapsed = 0f;
            float window  = 1.5f;   // seconds to react
            while (_noBuenoWindowOpen && elapsed < window)
            {
                elapsed += Time.deltaTime;

                // AI players auto-decide No Bueno during the window
                foreach (var p in _players)
                {
                    if (p.Type == PlayerType.AI && p != active)
                        AIConsiderNoBueno(p, card);
                }

                yield return null;
            }
            _noBuenoWindowOpen = false;

            if (_noBuenoWasPlayed && _noBuenoBlocker != null && _noBuenoCard != null)
            {
                // Check if the No Bueno itself can be countered (another player plays No Bueno)
                // In base implementation we resolve one level of No Bueno chaining
                string log = _resolver.ResolveNoBueno(_noBuenoBlocker, _noBuenoCard, card);
                GameEvents.OnLogMessage?.Invoke(log);
                GameEvents.OnHandChanged?.Invoke(_noBuenoBlocker);
                // card is now trashed; do NOT proceed with original effect
            }
            else
            {
                onProceed?.Invoke();
            }
        }

        // -------------------------------------------------------
        //  Action card resolution
        // -------------------------------------------------------
        private IEnumerator ResolveActionCard(Player caster, CardBase card,
                                               int preSelectedTarget, bool wasLastCard)
        {
            string log = "";

            switch (card)
            {
                // ---- Crafty Crow: pick a player then pick a card from their meal ----
                case CraftyCrowCard c:
                    yield return StartCoroutine(ResolveWithPlayerThenMealCard(
                        caster, card, "Choose a player to steal from:", wasLastCard,
                        (victim, stolen) => {
                            log = _resolver.ResolveCraftyCrow(caster, c, victim, stolen);
                        }));
                    break;

                // ---- Trash Panda: pick a card from Trash pile ----
                case TrashPandaCard c:
                    var trash = _deck.PeekTrash().ToList();
                    if (trash.Count == 0)
                    {
                        _deck.Trash(card);
                        log = $"🦝 Trash Panda: Trash pile is empty!";
                    }
                    else
                    {
                        bool done = false;
                        GameEvents.OnNeedCardFromTrash?.Invoke(
                            "Choose a card to retrieve from the Trash pile:", trash,
                            chosen => {
                                var (tpLog, _) = _resolver.ResolveTrashPanda(caster, c, chosen);
                                log  = tpLog;
                                done = true;
                            });
                        if (caster.Type == PlayerType.AI)
                        {
                            var best = AIPickBestFromTrash(caster, trash);
                            var (tpLog, _) = _resolver.ResolveTrashPanda(caster, c, best);
                            log = tpLog; done = true;
                        }
                        yield return new WaitUntil(() => done);
                    }
                    break;

                // ---- Food Fight ----
                case FoodFightCard c:
                    var clockwise = GetClockwiseOrderFrom(caster);
                    log = _resolver.ResolveFoodFight(caster, c, clockwise);
                    foreach (var p in clockwise) GameEvents.OnHandChanged?.Invoke(p);
                    break;

                // ---- Order Envy: pick a player ----
                case OrderEnvyCard c:
                    yield return StartCoroutine(ResolveWithPlayerTarget(
                        caster, c, "Choose a player to swap with:", wasLastCard,
                        target => {
                            log = _resolver.ResolveOrderEnvy(caster, c, target);
                        }));
                    break;

                default:
                    _deck.Trash(card);
                    log = $"⚠ Unknown action on card '{card.Name}'.";
                    break;
            }

            GameEvents.OnLogMessage?.Invoke(log);
            foreach (var p in _players)
            {
                GameEvents.OnHandChanged?.Invoke(p);
                GameEvents.OnScoreChanged?.Invoke(p, p.Score);
            }

            if (wasLastCard) TriggerGameEnd();
            else             _humanPlayedCard = true;
        }

        // ---- Helper: resolve after picking a target player ----
        private IEnumerator ResolveWithPlayerTarget(Player caster, CardBase card, string prompt,
                                                     bool wasLastCard, System.Action<Player> onTarget)
        {
            var targets = _players.Where(p => p != caster).ToList();
            bool done   = false;

            if (caster.Type == PlayerType.AI)
            {
                onTarget(AIPickTarget(caster));
                done = true;
            }
            else
            {
                GameEvents.OnNeedPlayerTarget?.Invoke(prompt, targets, target => {
                    onTarget(target);
                    done = true;
                });
            }
            yield return new WaitUntil(() => done);
        }

        // ---- Helper: resolve after picking player then card from their meal ----
        private IEnumerator ResolveWithPlayerThenMealCard(Player caster, CardBase card, string prompt,
                                                           bool wasLastCard,
                                                           System.Action<Player, CardBase> onResult)
        {
            bool done   = false;

            if (caster.Type == PlayerType.AI)
            {
                var victim  = AIPickTarget(caster);
                var stolen  = AIPickCardFromMeal(victim);
                if (stolen != null) onResult(victim, stolen);
                done = true;
            }
            else
            {
                var targets = _players.Where(p => p != caster && p.Meal.CardCount > 0).ToList();
                if (targets.Count == 0) { _deck.Trash(card); done = true; }
                else
                {
                    GameEvents.OnNeedPlayerAndMealCard?.Invoke(prompt, targets,
                        (victim, stolen) => {
                            onResult(victim, stolen);
                            done = true;
                        });
                }
            }
            yield return new WaitUntil(() => done);
        }

        // -------------------------------------------------------
        //  AI turn
        // -------------------------------------------------------
        private IEnumerator ExecuteAITurn(Player ai)
        {
            yield return new WaitForSeconds(0.8f); // thinking delay

            var decision = AIBrain.Decide(ai, _players, _deck);

            if (decision.cardIndex < 0 || decision.cardIndex >= ai.Hand.Count)
            {
                // Fallback: discard first card
                HumanDiscardsCard(0); // re-use discard path
                yield break;
            }

            var card = ai.Hand.GetAt(decision.cardIndex);
            bool isLastCard = ai.Hand.Count == 1;

            if (card.IsPlaceableInMeal)
            {
                int destIdx = decision.destPlayerIndex >= 0 ? decision.destPlayerIndex : ai.Index;

                // No Bueno interrupt (other AI/human players)
                //bool blocked = false;
                yield return StartCoroutine(NoBuenoInterruptWindow(ai, card, isLastCard, () => {}));
                if (_noBuenoWasPlayed) { _humanPlayedCard = true; yield break; }

                string log = _resolver.PlaceCardInMeal(ai, card, _players[destIdx]);
                GameEvents.OnLogMessage?.Invoke(log);
                var dest = _players[destIdx];
                GameEvents.OnMealScoreChanged?.Invoke(dest);
                GameEvents.OnScoreChanged?.Invoke(dest, dest.Score);
                GameEvents.OnHandChanged?.Invoke(ai);

                if (isLastCard) TriggerGameEnd();
                else            _humanPlayedCard = true;
            }
            else if (card is ActionCardBase)
            {
                yield return StartCoroutine(NoBuenoInterruptWindow(ai, card, isLastCard, () => {}));
                if (_noBuenoWasPlayed) { _humanPlayedCard = true; yield break; }

                ai.Hand.RemoveCard(card);
                GameEvents.OnHandChanged?.Invoke(ai);
                yield return StartCoroutine(ResolveActionCard(ai, card, decision.destPlayerIndex, isLastCard));
            }
            else
            {
                // Discard
                ai.Hand.RemoveCard(card);
                _deck.Trash(card);
                GameEvents.OnHandChanged?.Invoke(ai);
                GameEvents.OnLogMessage?.Invoke($"  {ai.Name} discarded '{card.Name}'.");
                if (isLastCard) TriggerGameEnd();
                else            _humanPlayedCard = true;
            }
        }

        private void AIConsiderNoBueno(Player aiPlayer, CardBase cardBeingPlayed)
        {
            if (!_noBuenoWindowOpen) return;
            if (AIBrain.ShouldPlayNoBueno(aiPlayer, cardBeingPlayed, _players))
            {
                for (int i = 0; i < aiPlayer.Hand.Count; i++)
                {
                    var c = aiPlayer.Hand.GetAt(i);
                    if (c is NoBuenoCard &&
                        _resolver.CanPlayNoBueno((NoBuenoCard)c, cardBeingPlayed, false))
                    {
                        aiPlayer.Hand.RemoveCard(c);
                        _noBuenoBlocker   = aiPlayer;
                        _noBuenoCard      = (NoBuenoCard)c;
                        _noBuenoWasPlayed = true;
                        _noBuenoWindowOpen = false;
                        GameEvents.OnLogMessage?.Invoke($"  {aiPlayer.Name} plays No Bueno!");
                        return;
                    }
                }
            }
        }

        // -------------------------------------------------------
        //  End of game
        // -------------------------------------------------------
        private void TriggerGameEnd()
        {
            _gameRunning = false;
            GameEvents.OnLogMessage?.Invoke("\n🎴 A player played their last card — GAME OVER!");
            StartCoroutine(FinalScoring());
        }

        private IEnumerator FinalScoring()
        {
            yield return new WaitForSeconds(0.5f);

            // Sort players by score descending
            var ranked = _players.OrderByDescending(p => p.Score).ToList();

            GameEvents.OnLogMessage?.Invoke("\n🏆 FINAL SCORES:");
            foreach (var p in ranked)
                GameEvents.OnLogMessage?.Invoke($"  {p.Name}: {p.Score} pts");

            // Check for tie among the top scorers
            int topScore   = ranked[0].Score;
            var tiedWinners = ranked.Where(p => p.Score == topScore).ToList();

            if (tiedWinners.Count == 1)
            {
                GameEvents.OnLogMessage?.Invoke($"\n🎉 WINNER: {ranked[0].Name} with {topScore} pts!");
                GameEvents.OnGameOver?.Invoke(ranked[0]);
            }
            else
            {
                // Tiebreaker: Food Fight between tied players
                GameEvents.OnLogMessage?.Invoke(
                    $"\n🤝 TIE between: {string.Join(", ", tiedWinners.Select(p => p.Name))}!");
                GameEvents.OnLogMessage?.Invoke("🍽 FOOD FIGHT TIEBREAKER!");

                yield return StartCoroutine(FoodFightTiebreaker(tiedWinners));
            }
        }

        private IEnumerator FoodFightTiebreaker(List<Player> tiedPlayers)
        {
            // Each tied player flips one card; highest ingredient wins
            var flips = new Dictionary<Player, CardBase>();
            foreach (var p in tiedPlayers)
            {
                var flipped = _deck.FlipTop();
                if (flipped != null)
                {
                    flips[p] = flipped;
                    GameEvents.OnLogMessage?.Invoke(
                        $"  {p.Name} flips: {flipped.Name} [{(flipped is IngredientCardBase @base ? @base.CardValue + "pt" : "0pt")}]");
                }
            }

            if (flips.Count == 0) { GameEvents.OnGameOver?.Invoke(tiedPlayers[0]); yield break; }

            int best = flips.Values.Max(c => c is IngredientCardBase @base ? @base.CardValue : 0);
            var winner = flips.FirstOrDefault(kv =>
                (kv.Value is IngredientCardBase @base? @base.CardValue : 0) == best).Key;

            // Shuffle flipped cards back
            _deck.ShuffleBackIn(flips.Values.ToList());

            GameEvents.OnLogMessage?.Invoke($"\n🎉 TIEBREAKER WINNER: {winner?.Name ?? tiedPlayers[0].Name}!");
            GameEvents.OnGameOver?.Invoke(winner ?? tiedPlayers[0]);
        }

        // -------------------------------------------------------
        //  Helpers
        // -------------------------------------------------------
        private void AdvanceToNextPlayer()
        {
            _currentIndex = (_currentIndex + 1) % _players.Count;
        }

        private List<Player> GetClockwiseOrderFrom(Player start)
        {
            var order = new List<Player>();
            int idx = _players.IndexOf(start);
            for (int i = 0; i < _players.Count; i++)
                order.Add(_players[(idx + i) % _players.Count]);
            return order;
        }

        public Player GetPlayer(int index) =>
            (index >= 0 && index < _players.Count) ? _players[index] : null;

        private bool IsHumanTurn() =>
            _gameRunning && CurrentPlayer.Type == PlayerType.Human && !_humanPlayedCard;

        // ---- AI helpers ----
        private Player AIPickTarget(Player ai) =>
            _players.Where(p => p != ai).OrderByDescending(p => p.Score).FirstOrDefault() ?? _players[0];

        private CardBase AIPickCardFromMeal(Player target)
        {
            // Prefer stealing Hot Sauce Boss, then highest ingredient
            var cards = target.Meal.Cards.ToList();
            return cards.OrderByDescending(c =>
                c is HotSauceBossCard ? 10 : 0).FirstOrDefault();
        }

        private CardBase AIPickBestFromTrash(Player ai, List<CardBase> trash)
        {
            return trash.OrderByDescending(c =>
                c is IngredientCardBase @base ? @base.CardValue + 10 :
                c is HotSauceBossCard? 50:
                c is ActionCardBase? 5: 0
            ).FirstOrDefault();
        }
    }

    // -------------------------------------------------------
    //  Player setup data (passed from setup UI to StartGame)
    // -------------------------------------------------------
    [System.Serializable]
    public struct PlayerSetupData
    {
        public string     playerName;
        public MealType   mealType;
        public PlayerType playerType;
    }
}