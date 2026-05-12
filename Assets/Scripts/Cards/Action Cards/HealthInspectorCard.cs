using System.Threading.Tasks;
using TacoVsBurrito;
using UnityEngine;

namespace TacoVsBurrito
{
    public class HealthInspectorCard : ActionCardBase
    {
        public override async void ExecuteAction()
        {
            await Task.Delay(700);
            resolver.ResolveHealthInspector(GameManager.Instance.CurrentPlayer);
        }
    }
}
