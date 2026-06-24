// ============================================================
//  AudioManager.cs  –  Event-driven SFX
//  Assign clips in the Inspector.
// ============================================================

using System;
using UnityEngine;

namespace TacoVsBurrito
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("SFX Clips")]
        [SerializeField] AudioClip bgMusic;
        [SerializeField] AudioClip sfxClick;
        [SerializeField] AudioClip sfxCardDeal;
        [SerializeField] AudioClip sfxCardFlip;
        [SerializeField] AudioClip sfxCardDraw;
        [SerializeField] AudioClip sfxLaugh1;
        [SerializeField] AudioClip sfxLaugh2;
        [SerializeField] AudioClip sfxCry1;
        [SerializeField] AudioClip sfxNoBueno;
        [SerializeField] AudioClip sfxCraftyCrow;
        [SerializeField] AudioClip sfxOrderEnvy;
        [SerializeField] AudioClip sfxHealthInspector;
        [SerializeField] AudioClip sfxWin;
        [SerializeField] AudioClip sfxTurnStart;

        [Range(0f, 1f)][SerializeField] float musicVolume = 0.8f;
        [Range(0f, 1f)][SerializeField] float sfxVolume = 1f;

        private AudioSource _src;
        private static AudioManager instance;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            _src = GetComponent<AudioSource>();
            _src.playOnAwake = false;
        }

        void Start()
        {
            PlayMusic(bgMusic);
        }

        private void OnEnable()
        {
            GameEvents.OnGUIClickSFX += OnGUIClicked;
            GameEvents.OnCardMovedSFX += OnCardDealed;
            GameEvents.OnCardFlippedSFX += OnCardFlipped;
            GameEvents.OnCardDrawSFX += OnCardDraw;
            GameEvents.OnLaugh1Sfx += OnLaugh1Sfx;
            GameEvents.OnLaugh2Sfx += OnLaugh2Sfx;
            GameEvents.OnCrySfx += OnCrySfx;
            GameEvents.OnCardClickedForActionTarget += OnCardClickedForActionTarget;
            GameEvents.OnTurnStarted += OnTurnStarted;
            GameEvents.OnHealthInspectorSFX += OnHealthInspector;
            GameEvents.OnNoBuenoPlayed += OnNoBuenoPlayed;
            GameEvents.OnCraftyCrowActionTargeted += OnCraftyCrowActionTargeted;
            GameEvents.OnOrderEnvyActionTargeted += OnOrderEnvyActionTargeted;
            GameEvents.OnGameFinished += OnGameFinished;
        }

        private void OnDisable()
        {
            GameEvents.OnGUIClickSFX -= OnGUIClicked;
            GameEvents.OnCardMovedSFX -= OnCardDealed;
            GameEvents.OnCardFlippedSFX -= OnCardFlipped;
            GameEvents.OnCardDrawSFX -= OnCardDraw;
            GameEvents.OnLaugh1Sfx -= OnLaugh1Sfx;
            GameEvents.OnLaugh2Sfx -= OnLaugh2Sfx;
            GameEvents.OnCrySfx -= OnCrySfx;
            GameEvents.OnCardClickedForActionTarget -= OnCardClickedForActionTarget;
            GameEvents.OnTurnStarted -= OnTurnStarted;
            GameEvents.OnHealthInspectorSFX -= OnHealthInspector;
            GameEvents.OnNoBuenoPlayed -= OnNoBuenoPlayed;
            GameEvents.OnCraftyCrowActionTargeted -= OnCraftyCrowActionTargeted;
            GameEvents.OnOrderEnvyActionTargeted -= OnOrderEnvyActionTargeted;
            GameEvents.OnGameFinished -= OnGameFinished;
        }

        private void OnCardDealed() => PlaySFX(sfxCardDeal);
        private void OnCardFlipped() => PlaySFX(sfxCardFlip);
        private void OnCardDraw() => PlaySFX(sfxCardDraw);
        private void OnLaugh1Sfx() => PlaySFX(sfxLaugh1);
        private void OnLaugh2Sfx() => PlaySFX(sfxLaugh2);
        private void OnCrySfx() => PlaySFX(sfxCry1);
        private void OnCardClickedForActionTarget(CardBase card) => PlaySFX(sfxCardDeal);
        private void OnGUIClicked() => PlaySFX(sfxClick);
        private void OnTurnStarted(PlayerBase player) => PlaySFX(sfxTurnStart);
        private void OnHealthInspector() => PlaySFX(sfxHealthInspector);
        private void OnNoBuenoPlayed(NoBuenoCard card) => PlaySFX(sfxNoBueno);
        private void OnCraftyCrowActionTargeted(TargetTypeContext ctx) => PlaySFX(sfxCraftyCrow);
        private void OnOrderEnvyActionTargeted(TargetTypeContext ctx) => PlaySFX(sfxOrderEnvy);
        private void OnGameFinished() => PlaySFX(sfxWin);

        private void PlayMusic(AudioClip clip)
        {
            if (clip && _src)
            {
                _src.Stop();
                _src.clip = clip;
                _src.volume = musicVolume;
                _src.loop = true;
                _src.Play();
            }
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip && _src) _src.PlayOneShot(clip, sfxVolume);
        }
    }
}