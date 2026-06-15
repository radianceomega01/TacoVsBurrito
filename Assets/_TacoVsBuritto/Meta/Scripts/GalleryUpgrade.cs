using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TacoVsBurrito
{
    public class GalleryUpgrade : MonoBehaviour
    {
        [SerializeField] private List<GalleryObjectAnimation> upgradeObjects;
        [SerializeField] private float staggerDelay = 0.15f;

        void Start()
        {
            PlayAll();
        }

        public void PlayAll()
        {
            ResetAll();
            StartCoroutine(PlaySequence());
        }
        private void ResetAll()
        {
            foreach (var item in upgradeObjects)
            {
                item.Reset();
            }
        }
        private IEnumerator PlaySequence()
        {
            foreach (var item in upgradeObjects)
            {
                item.PlayAnimation();
                yield return new WaitForSeconds(staggerDelay);
            }
        }
    }
}
