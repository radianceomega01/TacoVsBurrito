using System.Collections.Generic;
using UnityEngine;

namespace TacoVsBurrito
{
    public class FoodFightDrawPile : MonoBehaviour
    {
        List<CardBase> _drawPile = new();

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
        }
    }
}
