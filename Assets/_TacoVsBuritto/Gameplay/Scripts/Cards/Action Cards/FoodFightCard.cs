using System;
using System.Threading.Tasks;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class FoodFightCard : ActionCardBase, ITargetTypeAction
    {
        const int EXECUTION_DELAY_IN_MS = 200;
        GameManager gameManager;
        public override void ExecuteAction()
        {
            //GameEvents.OnStartNoBuenoInterruptWindow?.Invoke(this);
            
            GameEvents.OnFoodFightAction?.Invoke(this);
        }

        public async void OnActionTargeted(TargetTypeContext targetTypeContext)
        {
            GameEvents.OnFoodFightOver?.Invoke();
            await Task.Delay(EXECUTION_DELAY_IN_MS);
            GameManager.Instance.GetTrashPile().Trash(this);
            resolver.ResolveFoodFight(targetTypeContext.caster, targetTypeContext.cardTargeted);
        }

        public async void ResolveAction()
        {
            GameEvents.OnFoodFightOver?.Invoke();
            await Task.Delay(EXECUTION_DELAY_IN_MS);
            GameManager.Instance.GetTrashPile().Trash(this);
            GameEvents.OnFoodFightAction?.Invoke(this);
        }

        public override bool CanExecuteAction()
        {
            GameManager gameManager = GameManager.Instance;
            if(gameManager.GetDrawPile().IsDrawPileEmpty || 
                gameManager.GetDrawPile().PileCards.Count < gameManager.Players.Count)
            {
                GameEvents.OnLogMessage("FoodFight cancelled due to insufficient cards!");
                return false;
            }
            return true;
        }
        
        public override TurnState GetStateOnTrashed() => TurnState.NoBuenoWindowPhase;
    }
}
