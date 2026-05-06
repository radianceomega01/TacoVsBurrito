using Unity.VisualScripting;
using UnityEngine;
namespace TacoVsBurrito
{
    public class AIPlayer : PlayerBase
    {
        private AIBrain aIBrain;

        void Awake()
        {
            aIBrain = transform.AddComponent<AIBrain>();
        }

        public AIBrain GetBrain() => aIBrain;
    }
}
