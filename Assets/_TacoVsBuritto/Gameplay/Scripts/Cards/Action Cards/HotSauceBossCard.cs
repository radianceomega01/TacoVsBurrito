using TacoVsBurrito;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HotSauceBossCard : ActionCardBase, IMealTypeAction
    {
        [Header("Fields")]
        [SerializeField] TextMeshProUGUI ValueTxtField;

        public override int FEEL_ANIM_DELAY_IN_MS => 0;
        private const int VALUE_MULTIPLIER = 2;
        
        protected override void Start()
        {
            base.Start();
            ValueTxtField.text = $"X{VALUE_MULTIPLIER}";
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            GameEvents.OnTurnEnded(GameplayManager.Instance.CurrentPlayer);
        }
        public override bool CanExecuteAction() => false;
        public int GetModifiedMealScore(int currentScore) => currentScore * VALUE_MULTIPLIER;
        public override TurnState GetStateOnPlayed() => TurnState.SkipPhase;
        public override void PlayCardPlayedEffect(){}
    }
}
