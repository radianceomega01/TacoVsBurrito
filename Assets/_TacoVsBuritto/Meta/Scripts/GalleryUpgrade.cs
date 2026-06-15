using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TacoVsBurrito
{
    public class GalleryUpgrade : MonoBehaviour
    {
        [SerializeField] private List<GalleryObjectAnimation> upgradeObjects;
        [SerializeField] private float staggerDelay = 0.15f;

        public void PlayAll()
        {
            StartCoroutine(PlaySequence());
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
