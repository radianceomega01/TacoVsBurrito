using TacoVsBurrito;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class TummyAcheCard : ActionCardBase, IMealTypeAction, IValueTypeCard
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

        public override void ExecuteAction()
        {
            GameEvents.OnTurnEnded(GameplayManager.Instance.CurrentPlayer);
        }
        public override bool CanExecuteAction() => false;
        public int GetModifiedMealScore(int currentScore) => currentScore + cardValue;
        public override TurnState GetStateOnPlayed() => TurnState.SkipPhase;

        public int GetValue() => cardValue;
    }
}
