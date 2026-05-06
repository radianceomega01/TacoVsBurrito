using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class AIPlayer : PlayerBase
    {
        private AIBrain aIBrain;

        protected override void Awake()
        {
            base.Awake();
            aIBrain = transform.AddComponent<AIBrain>();
        }

        public AIBrain GetBrain() => aIBrain;
    }
}
