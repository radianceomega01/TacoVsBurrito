using TMPro;
using UnityEngine;
namespace TacoVsBurrito
{
    public class MinifiedPlayerView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI handCardsCountText;
        [SerializeField] TextMeshProUGUI mealValueText;

        public void EnableView(int mealValue, int handCardsCount)
        {
            gameObject.SetActive(true);
            handCardsCountText.SetText(handCardsCount.ToString());
            mealValueText.SetText(mealValue.ToString());
        }
        public void DisableView() => gameObject.SetActive(false);
    }
}
