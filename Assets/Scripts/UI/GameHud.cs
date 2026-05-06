// ============================================================
//  GameHUD.cs  –  Main in-game UI controller
//
//  Displays:
//  - Current playerBase indicator + draw pile count
//  - All playerBases' meals (face-up cards with score)
//  - Active playerBase's hand (clickable cards)
//  - Action log panel
//  - No Bueno interrupt button (reactive, any playerBase)
//  - Target selection overlays:
//      · Pick a playerBase
//      · Pick a card from a meal (Crafty Crow)
//      · Pick a card from Trash pile (Trash Panda)
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TacoVsBurrito
{
    public class GameHUD : MonoBehaviour
    {
        // -------------------------------------------------------
        [Header("Turn Info Bar")]
        public TextMeshProUGUI turnLabel;        // "PlayerBase X's Turn"
        public TextMeshProUGUI drawPileLabel;    // "Draw Pile: N cards"
        public TextMeshProUGUI statusLabel;      // "Playing…" / "Draw pile empty!"

        [Header("Hand Panel (active human playerBase)")]
        public Transform        handContainer;       // HorizontalLayoutGroup
        public GameObject       cardButtonPrefab;    // Button + Image + TMP label
        public TextMeshProUGUI  handOwnerLabel;      // "Your Hand"

        [Header("Meal Panels (one per playerBase, created at runtime)")]
        public Transform        mealsContainer;      // parent for all meal panels
        public GameObject       mealPanelPrefab;     // MealPanelUI component

        [Header("Log")]
        public TextMeshProUGUI  logText;
        public ScrollRect       logScroll;
        private const int MAX_LOG = 80;
        private List<string>    _logLines = new List<string>();

        [Header("No Bueno Interrupt")]
        public GameObject           noBuenoPanel;       // shown during interrupt window
        public Button               noBuenoButton;      // click to block
        public TextMeshProUGUI      noBuenoPromptLabel;

        [Header("Target Overlays")]
        public GameObject           targetOverlay;
        public TextMeshProUGUI      targetPromptLabel;
        public Transform            targetButtonsParent;
        public GameObject           targetButtonPrefab;

        [Header("Meal Card Select Overlay (Crafty Crow)")]
        public GameObject           mealCardOverlay;
        public TextMeshProUGUI      mealCardPromptLabel;
        public Transform            mealCardButtonsParent;
        public GameObject           mealCardButtonPrefab;

        [Header("Trash Pile Overlay (Trash Panda)")]
        public GameObject           trashOverlay;
        public TextMeshProUGUI      trashPromptLabel;
        public Transform            trashButtonsParent;
        public GameObject           trashButtonPrefab;

        [Header("Destination Meal Overlay")]
        public GameObject           destMealOverlay;
        public TextMeshProUGUI      destMealPromptLabel;
        public Transform            destMealButtonsParent;
        public GameObject           destMealButtonPrefab;

        // Runtime
        private Dictionary<int, MealPanelUI> _mealPanels = new Dictionary<int, MealPanelUI>();
        private int _pendingCardHandIndex = -1;   // hand index of card awaiting dest selection

        // -------------------------------------------------------
        private void Awake()
        {
            SubscribeEvents();
            HideAllOverlays();
        }

        private void OnDestroy() => UnsubscribeEvents();

        // -------------------------------------------------------
        //  Event subscriptions
        // -------------------------------------------------------
        private void SubscribeEvents()
        {
            GameEvents.OnGameStarted            += OnGameStarted;
            GameEvents.OnTurnStarted            += OnTurnStarted;
            GameEvents.OnTurnEnded              += _ => RefreshDrawCount();
            GameEvents.OnHandChanged            += OnHandChanged;
            GameEvents.OnCardPlacedInMeal       += (_, dest, __) => RefreshMeal(dest);
            GameEvents.OnMealCleared            += OnMealCleared;
            GameEvents.OnCardStolenFromMeal     += (_, victim, __) => RefreshMeal(victim);
            GameEvents.OnOrderEnvySwap          += (a, b) => { RefreshMeal(a); RefreshMeal(b); };
            GameEvents.OnScoreChanged           += (p, _) => RefreshMeal(p);
            GameEvents.OnLogMessage             += AppendLog;
            GameEvents.OnDrawPileEmpty          += OnDrawPileEmpty;
            GameEvents.OnGameOver               += OnGameOver;

            GameEvents.OnCardAboutToBePlayed    += OnCardAboutToBePlayed;
            GameEvents.OnNeedPlayerBaseTarget       += ShowPlayerBaseTargetOverlay;
            GameEvents.OnNeedCardFromMeal       += ShowMealCardOverlay;
            GameEvents.OnNeedCardFromTrash      += ShowTrashOverlay;
            GameEvents.OnNeedPlayerBaseAndMealCard  += ShowPlayerBaseThenMealCardFlow;
        }

        private void UnsubscribeEvents()
        {
            GameEvents.OnGameStarted            -= OnGameStarted;
            GameEvents.OnTurnStarted            -= OnTurnStarted;
            GameEvents.OnHandChanged            -= OnHandChanged;
            GameEvents.OnCardPlacedInMeal       -= (_, dest, __) => RefreshMeal(dest);
            GameEvents.OnMealCleared            -= OnMealCleared;
            GameEvents.OnLogMessage             -= AppendLog;
            GameEvents.OnGameOver               -= OnGameOver;
        }

        // -------------------------------------------------------
        //  Game started
        // -------------------------------------------------------
        private void OnGameStarted(List<PlayerBase> playerBases)
        {
            // Build one meal panel per playerBase
            foreach (Transform t in mealsContainer) Destroy(t.gameObject);
            _mealPanels.Clear();

            foreach (var p in playerBases)
            {
                var go    = Instantiate(mealPanelPrefab, mealsContainer);
                var panel = go.GetComponent<MealPanelUI>();
                panel.Setup(p);
                _mealPanels[p.Index] = panel;
            }

            RefreshDrawCount();
        }

        // -------------------------------------------------------
        //  Turn started
        // -------------------------------------------------------
        private void OnTurnStarted(PlayerBase playerBase)
        {
            turnLabel.text = $"{playerBase.Name}'s Turn ({playerBase.MealChoice})";
            RefreshDrawCount();

            // Show hand only for the current human playerBase
            if (playerBase is HumanPlayer)
                BuildHandUI(playerBase);
            else
                ClearHandUI($"{playerBase.Name} (AI) is thinking…");
        }

        // -------------------------------------------------------
        //  Hand UI
        // -------------------------------------------------------
        private void OnHandChanged(PlayerBase playerBase)
        {
            if (playerBase == GameManager.Instance.CurrentPlayer &&
                playerBase is HumanPlayer)
                BuildHandUI(playerBase);
        }

        private void BuildHandUI(PlayerBase playerBase)
        {
            ClearHandUI($"Your Hand ({playerBase.Hand.Count} cards):");

            for (int i = 0; i < playerBase.Hand.Count; i++)
            {
                int idx  = i;
                var card = playerBase.Hand.GetAt(i);
                var go   = Instantiate(cardButtonPrefab, handContainer);

                var img = go.GetComponent<Image>();
                var btn = go.GetComponent<Button>();
                var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

                string ptStr = card is IngredientCardBase ? $"+{((IngredientCardBase)card).CardValue} pt"
                             : card is TummyAcheCard  ? $"{((IngredientCardBase)card).CardValue} pt"
                             : card is HotSauceBossCard ? "×2"
                             : "None";
                if (lbl) lbl.text = $"{card.Name}\n{ptStr}";

                btn.onClick.AddListener(() => OnHandCardClicked(idx, card));
            }
        }

        private void ClearHandUI(string label = "")
        {
            foreach (Transform t in handContainer) Destroy(t.gameObject);
            if (handOwnerLabel) handOwnerLabel.text = label;
        }

        private void OnHandCardClicked(int idx, CardBase card)
        {
            var playerBase = GameManager.Instance.CurrentPlayer;
            if (playerBase is HumanPlayer) return;

            if (card.IsPlaceableInMeal)
            {
                // Ask where to place it (own meal or opponent's)
                _pendingCardHandIndex = idx;
                ShowDestinationOverlay(card);
            }
            else if (card is ActionCardBase)
            {
                // Action cards that need a target are handled inside GameManager via events
                GameManager.Instance.HumanPlaysActionCard(idx);
            }
        }

        // -------------------------------------------------------
        //  Destination meal overlay (who gets this ingredient?)
        // -------------------------------------------------------
        private void ShowDestinationOverlay(CardBase card)
        {
            destMealOverlay.SetActive(true);
            destMealPromptLabel.text = $"Place '{card.Name}' in which meal?";

            foreach (Transform t in destMealButtonsParent) Destroy(t.gameObject);

            var playerBases = GameManager.Instance.Players;
            foreach (var p in playerBases)
            {
                var pCopy = p;
                var go    = Instantiate(destMealButtonPrefab, destMealButtonsParent);
                var btn   = go.GetComponent<Button>();
                var lbl   = go.GetComponentInChildren<TextMeshProUGUI>();

                // Colour-code: own meal = green tint, opponent = normal
                if (go.TryGetComponent<Image>(out var img))
                    img.color = (p == GameManager.Instance.CurrentPlayer)
                        ? new Color(0.6f, 1f, 0.6f) : Color.white;

                bool isSelf = p == GameManager.Instance.CurrentPlayer;
                if (lbl) lbl.text = isSelf
                    ? $"Your {p.MealChoice} ({p.Score} pts)"
                    : $"{p.Name}'s {p.MealChoice} ({p.Score} pts)";

                btn.onClick.AddListener(() => {
                    destMealOverlay.SetActive(false);
                    GameManager.Instance.HumanPlacesCardInMeal(_pendingCardHandIndex, pCopy.Index);
                });
            }
        }

        // -------------------------------------------------------
        //  No Bueno interrupt
        // -------------------------------------------------------
        private void OnCardAboutToBePlayed(PlayerBase active, CardBase card, bool isLastCard,
                                            System.Action<PlayerBase, CardBase> noBuenoCallback)
        {
            if (isLastCard || card is HealthInspectorCard) return;

            // Check if any human playerBase holds a No Bueno
            bool anyHumanHasNoBueno = false;
            foreach (var p in GameManager.Instance.Players)
            {
                if (p is HumanPlayer && p != active)
                {
                    foreach (var c in p.Hand.Cards)
                        if (c is NoBuenoCard) { anyHumanHasNoBueno = true; break; }
                }
                if (anyHumanHasNoBueno) break;
            }

            if (!anyHumanHasNoBueno) return;

            noBuenoPanel.SetActive(true);
            noBuenoPromptLabel.text = $"'{active.Name}' is playing '{card.Name}'.\nPlay No Bueno?";

            noBuenoButton.onClick.RemoveAllListeners();
            noBuenoButton.onClick.AddListener(() => {
                noBuenoPanel.SetActive(false);
                // Find first human playerBase's No Bueno
                foreach (var p in GameManager.Instance.Players)
                {
                    if (p is HumanPlayer && p != active)
                    {
                        foreach (var c in p.Hand.Cards)
                        {
                            if (c is NoBuenoCard)
                            {
                                p.Hand.RemoveCard(c);
                                noBuenoCallback?.Invoke(p, c);
                                return;
                            }
                        }
                    }
                }
            });

            // Auto-dismiss after window closes (handled by NoBuenoWindowOpen flag in GameManager)
            StartCoroutine(AutoDismissNoBueno());
        }

        private System.Collections.IEnumerator AutoDismissNoBueno()
        {
            yield return new UnityEngine.WaitForSeconds(1.6f);
            noBuenoPanel.SetActive(false);
        }

        // -------------------------------------------------------
        //  PlayerBase target overlay
        // -------------------------------------------------------
        private void ShowPlayerBaseTargetOverlay(string prompt, List<PlayerBase> validTargets,
                                              System.Action<PlayerBase> callback)
        {
            targetOverlay.SetActive(true);
            targetPromptLabel.text = prompt;
            foreach (Transform t in targetButtonsParent) Destroy(t.gameObject);

            foreach (var p in validTargets)
            {
                var pCopy = p;
                var go    = Instantiate(targetButtonPrefab, targetButtonsParent);
                var btn   = go.GetComponent<Button>();
                var lbl   = go.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl) lbl.text = $"{p.Name}\n{p.MealChoice} | {p.Score} pts";
                btn.onClick.AddListener(() => {
                    targetOverlay.SetActive(false);
                    callback?.Invoke(pCopy);
                });
            }
        }

        // -------------------------------------------------------
        //  Meal card overlay (Crafty Crow – pick card from meal)
        // -------------------------------------------------------
        private void ShowMealCardOverlay(string prompt, List<CardBase> cards,
                                          System.Action<CardBase> callback)
        {
            mealCardOverlay.SetActive(true);
            mealCardPromptLabel.text = prompt;
            foreach (Transform t in mealCardButtonsParent) Destroy(t.gameObject);

            foreach (var card in cards)
            {
                var cCopy = card;
                var go    = Instantiate(mealCardButtonPrefab, mealCardButtonsParent);
                var btn   = go.GetComponent<Button>();
                var lbl   = go.GetComponentInChildren<TextMeshProUGUI>();
                var img   = go.GetComponent<Image>();
                btn.onClick.AddListener(() => {
                    mealCardOverlay.SetActive(false);
                    callback?.Invoke(cCopy);
                });
            }
        }

        // -------------------------------------------------------
        //  Trash pile overlay (Trash Panda)
        // -------------------------------------------------------
        private void ShowTrashOverlay(string prompt, List<CardBase> trashCards,
                                       System.Action<CardBase> callback)
        {
            trashOverlay.SetActive(true);
            trashPromptLabel.text = prompt;
            foreach (Transform t in trashButtonsParent) Destroy(t.gameObject);

            foreach (var card in trashCards)
            {
                var cCopy = card;
                var go    = Instantiate(trashButtonPrefab, trashButtonsParent);
                var btn   = go.GetComponent<Button>();
                var lbl   = go.GetComponentInChildren<TextMeshProUGUI>();
                var img   = go.GetComponent<Image>();
                btn.onClick.AddListener(() => {
                    trashOverlay.SetActive(false);
                    callback?.Invoke(cCopy);
                });
            }
        }

        // -------------------------------------------------------
        //  Two-step: pick playerBase THEN pick card from their meal
        // -------------------------------------------------------
        private void ShowPlayerBaseThenMealCardFlow(string prompt, List<PlayerBase> targets,
                                                System.Action<PlayerBase, CardBase> callback)
        {
            ShowPlayerBaseTargetOverlay(prompt, targets, chosenPlayerBase => {
                var mealCards = chosenPlayerBase.Meal.Cards as IReadOnlyList<CardBase>;
                if (mealCards == null || mealCards.Count == 0)
                {
                    AppendLog($"  {chosenPlayerBase.Name}'s meal is empty – Crafty Crow fizzles.");
                    callback?.Invoke(chosenPlayerBase, null);
                    return;
                }
                ShowMealCardOverlay($"Steal which card from {chosenPlayerBase.Name}'s meal?",
                    new List<CardBase>(mealCards),
                    chosenCard => callback?.Invoke(chosenPlayerBase, chosenCard));
            });
        }

        // -------------------------------------------------------
        //  Meal panel refresh
        // -------------------------------------------------------
        private void RefreshMeal(PlayerBase p)
        {
            if (_mealPanels.TryGetValue(p.Index, out var panel))
                panel.Refresh(p);
        }

        private void OnMealCleared(PlayerBase p)
        {
            if (_mealPanels.TryGetValue(p.Index, out var panel))
                panel.Refresh(p);
        }

        // -------------------------------------------------------
        //  Draw pile counter
        // -------------------------------------------------------
        private void RefreshDrawCount()
        {
            int count = GameManager.Instance.Deck.DrawCount;
            drawPileLabel.text = count > 0 ? $"Draw Pile: {count}" : "Draw Pile: EMPTY";
            if (statusLabel)
                statusLabel.text = GameManager.Instance.IsDrawPileEmpty
                    ? "⚠ Draw pile empty – skip draw step!" : "";
        }

        private void OnDrawPileEmpty()
        {
            RefreshDrawCount();
            AppendLog("📭 Draw pile exhausted. PlayerBases skip the draw step from now on.");
        }

        // -------------------------------------------------------
        //  Game over
        // -------------------------------------------------------
        private void OnGameOver(PlayerBase winner)
        {
            turnLabel.text = $"🏆 {winner.Name} WINS!";
            ClearHandUI();
            HideAllOverlays();
        }

        // -------------------------------------------------------
        //  Log
        // -------------------------------------------------------
        private void AppendLog(string msg)
        {
            _logLines.Add(msg);
            if (_logLines.Count > MAX_LOG) _logLines.RemoveAt(0);
            logText.text = string.Join("\n", _logLines);
            Canvas.ForceUpdateCanvases();
            if (logScroll) logScroll.verticalNormalizedPosition = 0f;
        }

        // -------------------------------------------------------
        private void HideAllOverlays()
        {
            if (noBuenoPanel)   noBuenoPanel.SetActive(false);
            if (targetOverlay)  targetOverlay.SetActive(false);
            if (mealCardOverlay)mealCardOverlay.SetActive(false);
            if (trashOverlay)   trashOverlay.SetActive(false);
            if (destMealOverlay)destMealOverlay.SetActive(false);
        }
    }

    // ============================================================
    //  MealPanelUI  –  Shows one playerBase's meal + score
    //  Attach to mealPanelPrefab
    // ============================================================
    public class MealPanelUI : MonoBehaviour
    {
        public TextMeshProUGUI  playerBaseNameLabel;
        public TextMeshProUGUI  scoreLabel;
        public TextMeshProUGUI  multiplierLabel;    // shows "×2" or "×4" if Hot Sauce Boss
        public Transform        cardListParent;      // VerticalLayoutGroup
        public GameObject       mealCardChipPrefab; // small label chip per card

        public void Setup(PlayerBase p)
        {
            playerBaseNameLabel.text = $"{p.Name}\n({p.MealChoice})";
            Refresh(p);
        }

        public void Refresh(PlayerBase p)
        {
            scoreLabel.text = $"{p.Score} pts";

            int hsb = p.Meal.HotSauceBossCardCount;
            if (multiplierLabel)
                multiplierLabel.text = hsb == 0 ? "" : hsb == 1 ? "×2!" : "×4!!";

            // Rebuild card chips
            foreach (Transform t in cardListParent) Destroy(t.gameObject);
            foreach (var card in p.Meal.Cards)
            {
                var go  = Instantiate(mealCardChipPrefab, cardListParent);
                var lbl = go.GetComponentInChildren<TextMeshProUGUI>();
                var img = go.GetComponent<Image>();
                
            }
        }
    }
}