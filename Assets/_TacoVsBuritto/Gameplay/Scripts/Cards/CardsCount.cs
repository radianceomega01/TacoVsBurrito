using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CardsCount : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI countTxt;

        public void ShowCount(int count)
        {
            if(count >= 2) //Start showing count from 2, as 1 card doesn't need a count
            {
                gameObject.SetActive(true);
                UpdateCountTxt(count.ToString());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void DisableCount() => gameObject.SetActive(false);
        void UpdateCountTxt(string count) => countTxt.SetText(count);

    }
}
