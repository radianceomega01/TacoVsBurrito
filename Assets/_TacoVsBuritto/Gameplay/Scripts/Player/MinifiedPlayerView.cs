using TMPro;
using UnityEngine;
namespace TacoVsBurrito
{
    public class MinifiedPlayerView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameTxt;
        [SerializeField] TextMeshProUGUI handCardsCountText;
        [SerializeField] TextMeshProUGUI mealValueText;
        [SerializeField] Transform foodFightCardPosTransform;

        public void EnableView(int mealValue, int handCardsCount)
        {
            gameObject.SetActive(true);
            handCardsCountText.SetText(handCardsCount.ToString());
            mealValueText.SetText(mealValue.ToString());
        }
        public void DisableView() => gameObject.SetActive(false);
        public void SetName(string name) => nameTxt.SetText(name);
        public Vector3 GetFoodFightCardPosition() => foodFightCardPosTransform.position;
    }
}
