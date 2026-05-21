using TacoVsBurrito;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TummyAcheCard : ActionCardBase, IMealTypeAction
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

        public int GetModifiedMealScore(int currentScore) => currentScore + cardValue;
        public override TurnState GetStateOnTrashed() => TurnState.SkipPhase;
    }
}
