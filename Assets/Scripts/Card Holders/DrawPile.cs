using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace TacoVsBurrito
{
    public class DrawPile : MonoBehaviour
    {
        [SerializeField] Transform cardsParent;
        [SerializeField] Button drawBtn;
        private List<CardBase> _drawPile = new();
        private const int STARTING_HAND_SIZE = 5;

        public int DrawCount => _drawPile.Count;
        public bool IsDrawPileEmpty => _drawPile.Count == 0;


        void Awake()
        {
            GameEvents.OnShuffleCards += ManageCardShuffle;
            GameEvents.OnDistributeCards += DealStartingHand;
            GameEvents.OnTurnStateChanged += ManageTurnStateChanged;
        }

        void OnDestroy()
        {
            GameEvents.OnShuffleCards -= ManageCardShuffle;
            GameEvents.OnDistributeCards -= DealStartingHand;
            GameEvents.OnTurnStateChanged -= ManageTurnStateChanged;
        }

        void Start()
        {
            InitDrawPile();
        }

        void InitDrawPile()
        {
            for (int i = 0; i < cardsParent.childCount; i++)
            {
                if (!cardsParent.GetChild(i).TryGetComponent<CardBase>(out var card))
                    continue;
                _drawPile.Add(card);
            }
        }

        void ManageCardShuffle()
        {
            Shuffle(_drawPile);
        }

        public CardBase Draw()
        {
            if (_drawPile.Count == 0)
                return null;
            var c = _drawPile[^1];
            _drawPile.RemoveAt(_drawPile.Count - 1);
            return c;
        }
        public List<CardBase> DrawMany(int count)
        {
            var result = new List<CardBase>();
            for (int i = 0; i < count; i++)
            {
                var c = Draw();
                if (c != null) result.Add(c); else break;
            }
            return result;
        }

        void DealStartingHand(List<PlayerBase> players)
        {
            int cardsDistributed = 0;
            int playerIndex = 0;
            while (cardsDistributed < players.Count * STARTING_HAND_SIZE)
            {
                var card = Draw();

                if (card is HealthInspectorCard)
                {
                    // Move to bottom of deck
                    _drawPile.Insert(0, card);
                    card.ChangeSiblingIndex(0);
                    continue;                               // draw again
                }
                players[playerIndex % players.Count].Hand.AddCard(card);
                cardsDistributed++;
                playerIndex++;
            }
        }

        void Shuffle(List<CardBase> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            // Sync hierarchy order with list order
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ChangeSiblingIndex(i);
            }
        }

        public void OnDrawBtnClicked()
        {
            if (GameManager.Instance.CurrentPlayer is not SelfPlayer)
                return;

            if (!IsDrawPileEmpty)
            {
                CardBase drawnCard = Draw();

                if (IsDrawPileEmpty)
                {
                    // Draw pile just became empty
                    GameEvents.OnDrawPileEmpty?.Invoke();
                    GameEvents.OnLogMessage?.Invoke(
                        "📭 Draw pile is empty! PlayerBases now skip the draw step.");
                }
                GameManager.Instance.CardDrawnFromPile(drawnCard);
            }
            else
            {
                GameEvents.OnDrawPhaseSkipped?.Invoke();
                GameEvents.OnLogMessage?.Invoke($"  (Draw pile empty – skip draw step)");
            }
        }

        void TogglePileInteraction(bool value) => drawBtn.interactable = value;

        void ManageTurnStateChanged(TurnState turnState, PlayerBase player)
        {
            if(turnState == TurnState.Draw && player is SelfPlayer)
            {
                TogglePileInteraction(true);
            }
            else
                TogglePileInteraction(false);
        }

        /// Flip the top card from the draw pile (Food Fight).
        /// Returns null if empty. Caller is responsible for placing it back or keeping it.
        public CardBase FlipTop() => Draw();

        /// Shuffle a list of cards back into the draw pile (Food Fight leftovers).
        public void ShuffleBackIn(List<CardBase> cards)
        {
            _drawPile.AddRange(cards);
            Shuffle(_drawPile);
        }
    }
}
