using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TacoVsBurrito
{
    public class EmojiHolder : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI emojiTxt;

        private const float HOLDER_SCALE = 1f;
        private const float SCALE_TIME_IN_SECS = 0.5f;
        private const int DURATION_IN_MS = 1500;
        private const int FEEDBACK_DEALY_IN_MS = 500;

        public void ShowEmoji(EmojiType emojiType)
        {
            switch (emojiType)
            {
                case EmojiType.Sad:
                emojiTxt.SetText("<sprite index=10>");
                break;
                case EmojiType.Evil:
                emojiTxt.SetText("<sprite index=11>");
                break;
                case EmojiType.Laughing:
                emojiTxt.SetText("<sprite index=13>");
                break;

            }
            DisplayWithEffect(emojiType);
        }
        private async void DisplayWithEffect(EmojiType emojiType)
        {
            if(emojiType == EmojiType.Sad)
            {
                await Task.Delay(FEEDBACK_DEALY_IN_MS + DURATION_IN_MS);
            }
            else
            {
                await Task.Delay(FEEDBACK_DEALY_IN_MS);
            }
            transform.DOScale(HOLDER_SCALE,SCALE_TIME_IN_SECS).SetEase(Ease.OutElastic);
            PlaySfx(emojiType);
            await Task.Delay(DURATION_IN_MS);
            transform.DOScale(0f,SCALE_TIME_IN_SECS).SetEase(Ease.OutElastic);
        }

        private void PlaySfx(EmojiType type)
        {
            switch (type)
            {
                case EmojiType.Sad:
                GameEvents.OnCrySfx?.Invoke();
                break;
                case EmojiType.Evil:
                GameEvents.OnLaugh1Sfx?.Invoke();
                break;
                case EmojiType.Laughing:
                GameEvents.OnLaugh2Sfx?.Invoke();
                break;

            }
        }
    }
    public enum EmojiType
    {
        Sad,
        Evil,
        Laughing,
    }
}
