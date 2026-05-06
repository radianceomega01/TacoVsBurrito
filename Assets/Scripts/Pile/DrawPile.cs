using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace TacoVsBurrito
{
    public class DrawPile : MonoBehaviour
    {
        private List<CardBase> _drawPile = new();
        private const int STARTING_HAND_SIZE = 5;

        public int DrawCount  => _drawPile.Count;
        public bool IsDrawPileEmpty => _drawPile.Count == 0;

        void Awake()
        {
            GameEvents.OnCardShuffled += ManageCardShuffle;
        }

        void OnDestroy()
        {
            GameEvents.OnCardShuffled -= ManageCardShuffle;
        }

        void Start()
        {
            InitDrawPile();
        }

        void InitDrawPile()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (!transform.GetChild(i).TryGetComponent<CardBase>(out var card))
                    continue;
                _drawPile.Add(card);
            }
            Debug.Log(_drawPile.Count);
        }
        
        public void ManageCardShuffle()
        {
            Shuffle(_drawPile);
            Debug.Log($"[DeckManager] Built deck: {_drawPile.Count} cards.");
            InitiateCardDistribution();
            
        }

        public CardBase Draw()
        {
            if (_drawPile.Count == 0) return null;
            var c = _drawPile[0];
            _drawPile.RemoveAt(0);
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

        public void DealStartingHand(List<PlayerBase> players)
        {
            int cardsDistributed = 0;
            int playerIndex = 0;
            while (cardsDistributed <= players.Count* STARTING_HAND_SIZE)
            {
                var card = Draw();

                if (card is HealthInspectorCard)
                {
                    // Shuffle back
                    int insertAt = Random.Range(0, _drawPile.Count + 1);
                    _drawPile.Insert(insertAt, card);
                    Debug.Log("[DeckManager] Health Inspector removed from starting hand and reshuffled.");
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
                list[i].transform.SetSiblingIndex(i);
            }
        }

        async void InitiateCardDistribution()
        {
            await Task.Delay(1500);
            GameEvents.OnCardDistributed?.Invoke();
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
