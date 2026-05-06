using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public abstract class CardBase: MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] string cardName = "Unnamed Card";
        [SerializeField]  string DescriptionText = "";
        [SerializeField]  bool isPlaceableInMeal = false;
        [SerializeField] bool isBlockable = false;

        [Header("Fields")]
        [SerializeField] protected TextMeshProUGUI nameTxtField;
        [SerializeField] protected TextMeshProUGUI DescriptionTxtField;

        public string Name { get { return cardName; } }

        // Common helpers
        public virtual bool IsPlaceableInMeal => isPlaceableInMeal;
        public virtual bool IsBlockable => isBlockable;

        public virtual int GetModifiedMealScore(int currentScore) { return currentScore; }

        protected virtual void Start()
        {
            nameTxtField.text = cardName;
            DescriptionTxtField.text = DescriptionText;
        }
    }
}