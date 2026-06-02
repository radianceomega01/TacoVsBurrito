// ============================================================
//  AudioManager.cs  –  Event-driven SFX
//  Assign clips in the Inspector.
// ============================================================

using UnityEngine;

namespace TacoVsBurrito
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("SFX Clips")]
        public AudioClip sfxCardDraw;
        public AudioClip sfxPlaceIngredient;
        public AudioClip sfxPlaceTummyAche;
        public AudioClip sfxHotSauceBoss;
        public AudioClip sfxNoBueno;
        public AudioClip sfxCraftyCrow;
        public AudioClip sfxTrashPanda;
        public AudioClip sfxFoodFight;
        public AudioClip sfxOrderEnvy;
        public AudioClip sfxHealthInspector;
        public AudioClip sfxWin;
        public AudioClip sfxTurnStart;

        [Range(0f, 1f)] public float volume = 0.85f;

        private AudioSource _src;

        private void OnEnable()
        {
            _src = GetComponent<AudioSource>();
            _src.playOnAwake = false;

            GameEvents.OnCardDrawn          += (_, __) => Play(sfxCardDraw);
            GameEvents.OnTurnStarted        += _        => Play(sfxTurnStart);
            GameEvents.OnHealthInspector    += ()        => Play(sfxHealthInspector);
            GameEvents.OnNoBuenoPlayed      += (card)        => Play(sfxNoBueno);
            GameEvents.OnCraftyCrowActionTargeted += (TargetTypeContext ctx) => Play(sfxCraftyCrow);
            GameEvents.OnOrderEnvyAction         += _        => Play(sfxOrderEnvy);
            GameEvents.OnGameOver           += _        => Play(sfxWin);

            // GameEvents.OnCardPlacedInMeal += (_, _, card) =>
            // {
            //     Play(card is TummyAcheCard  ? sfxPlaceTummyAche
            //        : card is HotSauceBossCard ? sfxHotSauceBoss
            //        :                                              sfxPlaceIngredient);
            // };
        }

        private void Play(AudioClip clip)
        {
            if (clip && _src) _src.PlayOneShot(clip, volume);
        }
    }
}