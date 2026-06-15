using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TacoVsBurrito
{
    public class GalleryObjectAnimation : MonoBehaviour
    {
        [SerializeField] private List<AnimationData> animations;

        private Sequence sequence;

        public void PlayAnimation()
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();

            foreach (var anim in animations)
            {
                Tween tween = CreateTween(anim);

                if (tween != null)
                {
                    sequence.Insert(anim.delay, tween);
                }
            }

            sequence.Play();
        }

        private Tween CreateTween(AnimationData data)
        {
            switch (data.animationType)
            {
                case AnimationType.Scale:
                    return transform.DOScale(data.vectorValue, data.duration)
                        .SetEase(data.ease);

                case AnimationType.Move:
                    return transform.DOLocalMove(data.vectorValue, data.duration)
                        .SetEase(data.ease);

                case AnimationType.Rotate:
                    return transform.DOLocalRotate(data.vectorValue, data.duration)
                        .SetEase(data.ease);

                default:
                    return null;
            }
        }
        [Serializable]
        public class AnimationData
        {
            public AnimationType animationType;

            public Vector3 vectorValue;

            public float duration = 0.5f;

            public float delay = 0f;

            public Ease ease = Ease.OutBack;
        }

        public enum AnimationType
        {
            Scale,
            Move,
            Rotate
        }
    }
}
