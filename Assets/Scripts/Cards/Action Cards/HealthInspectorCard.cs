using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HealthInspectorCard : ActionCardBase
    {
        public override void ExecuteAction()
        {
            resolver.ResolveHealthInspector(GameManager.Instance.CurrentPlayer);
        }
    }
}
