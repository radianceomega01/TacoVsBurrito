using UnityEngine;
namespace TacoVsBurrito
{
    public class SelfPlayer : HumanPlayer
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
