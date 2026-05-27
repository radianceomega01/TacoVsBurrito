using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class CardsCount : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI countTxt;

        public int Count {get; private set;}

        public void IncrementCount()
        {
            if(Count == 0)
            {
                gameObject.SetActive(true);
            }
            Count++;
            UpdateCountTxt();
        }
        public void DecrementCount()
        {
            Count--;
            UpdateCountTxt();
            if(Count == 0)
            {
                gameObject.SetActive(false);
            }
        }

        void UpdateCountTxt() => countTxt.SetText(Count.ToString());

    }
}
