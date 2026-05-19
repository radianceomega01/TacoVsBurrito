using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class FoodFightHandler
    {
        DrawPile drawPile;

        private void Start() {
            drawPile = GameManager.Instance.GetDrawPile();
            
        }

        void EnableFoodFightDrawPile()
        {
            drawPile.enabled = false;
            drawPile.AddComponent<FoodFightDrawPile>();
        }
        void DisableFoodFIghtDrawPile()
        {
            drawPile.enabled = true;
            //drawPile.Component
        }
        
    }
}
