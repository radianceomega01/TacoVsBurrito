using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace TacoVsBurrito
{
    public class GlowBGUI : MonoBehaviour
    {
        [SerializeField] Image glowImage;
        const float fadeDuration = 1f;

        private void Start()
        {
            ResetColor();
        }
        public void ShowEffect()
        {
            // 2. Animate to 1, loop infinitely, and alternate backwards
            glowImage.DOFade(1f, fadeDuration)
                   .SetLoops(-1, LoopType.Yoyo)
                   .SetEase(Ease.Linear);
        }

        public void Reset()
        {
            glowImage.DOKill();
            ResetColor();
        }
        void ResetColor()
        {
            glowImage.color = new Color(glowImage.color.r, glowImage.color.g, glowImage.color.b, 0f);
        }
    }
}
