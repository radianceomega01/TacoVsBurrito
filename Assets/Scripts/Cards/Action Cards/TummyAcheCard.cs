using TacoVsBurrito;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TummyAcheCard : ActionCardBase
    {
        [SerializeField] int cardValue = -1;
        
        [Header("Fields")]
        [SerializeField] TextMeshProUGUI ValueTxtField;
        public int CardValue { get { return cardValue; } }

        protected override void Start()
        {
            base.Start();
            ValueTxtField.text = cardValue.ToString();
        }

        public override void ExecuteAction() { }

        public override int GetModifiedMealScore(int currentScore) => currentScore + cardValue;
    }
}
