
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TacoVsBurrito
{
    public abstract class CardBase : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IGlowEntity
    {
        [Header("Identity")]
        [SerializeField] string cardName = "Unnamed Card";
        [SerializeField] string DescriptionText = "";

        [Header("Fields")]
        [SerializeField] protected TextMeshProUGUI nameTxtField;
        [SerializeField] protected TextMeshProUGUI DescriptionTxtField;
        [SerializeField] protected Transform backFaceImage;
        [SerializeField] protected GlowBGUI glowBG;


        private const float DRAG_SCALE = 1.5f;
        private const float TIME_TO_MOVE_IN_SECS = 0.35f;
        private const float TIME_TO_SCALE_IN_SECS = 0.25f;

        private RectTransform _rectTransform;
        private Canvas canvas;
        private CanvasGroup _canvasGroup;
        private Vector3 _originalScale;

        private ICardPickupTarget tempPickupTarget;
        private bool wasInteractionEnabledBeforeDrag;
        private InteractionType interactionType = InteractionType.Drag;


        public string Name { get { return cardName; } }


        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();

            GameEvents.OnCardDragBegin += DisableInteraction;
            GameEvents.OnCardDragEnd += ResumeInteraction; ;
        }

        protected virtual void OnDestroy()
        {
            GameEvents.OnCardDragBegin -= DisableInteraction;
            GameEvents.OnCardDragEnd -= ResumeInteraction;
        }

        protected virtual void Start()
        {
            nameTxtField.text = cardName;
            DescriptionTxtField.text = DescriptionText;
        }

        public void ChangePosition(Vector3 newPos)
        {
            transform.DOMove(newPos, TIME_TO_MOVE_IN_SECS);
        }
        public void ChangeRotation(Quaternion angle)
        {
            transform.DORotateQuaternion(angle, TIME_TO_MOVE_IN_SECS);
        }
        public void ChangeParent(Transform newParent)
        {
            transform.SetParent(newParent);
        }
        public void ChangeSiblingIndex(int newIndex)
        {
            transform.SetSiblingIndex(newIndex);
        }

        // =========================================================
        // Click 
        // =========================================================
        public void OnPointerDown(PointerEventData eventData)
        {
            if(interactionType == InteractionType.Drag) return;

            GameEvents.OnCardClickedForActionTarget?.Invoke(this);
        }
        // =========================================================
        // BEGIN DRAG
        // =========================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(interactionType == InteractionType.Click) return;

            GameEvents.OnCardDragBegin?.Invoke();

            _originalScale = transform.localScale;
            _rectTransform.rotation = Quaternion.Euler(0, 0, 0);

            // Allow raycasts to pass through while dragging
            _canvasGroup.blocksRaycasts = false;

            // Optional visual feedback
            transform.localScale = Vector3.one * DRAG_SCALE;
            transform.SetParent(canvas.transform); // Move to top-level canvas to avoid clipping

            StartCoroutine(PickCardBeforeDrag(eventData));

        }

        IEnumerator PickCardBeforeDrag(PointerEventData eventData)
        {
            yield return null;
            GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;
            if (targetObject != null)
            {
                tempPickupTarget = targetObject.GetComponent<ICardPickupTarget>();
                tempPickupTarget?.PickCardBeforeDrag(this);
            }
        }
        // =========================================================
        // DRAG
        // =========================================================

        public void OnDrag(PointerEventData eventData)
        {
            if(interactionType == InteractionType.Click) return;

            _rectTransform.anchoredPosition +=
                eventData.delta / canvas.scaleFactor;
        }

        // =========================================================
        // END DRAG
        // =========================================================

        public void OnEndDrag(PointerEventData eventData)
        {
            if(interactionType == InteractionType.Click) return;

            GameEvents.OnCardDragEnd?.Invoke();
            _canvasGroup.blocksRaycasts = true;

            transform.localScale = _originalScale;

            GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;
            if (targetObject != null)
            {
                // ICardHolder cardHolder = targetObject.GetComponent<ICardHolder>();
                // if (cardHolder != null)
                // {
                //     currentCardHolder = cardHolder;
                // }
                ICardDropTarget dropTarget = targetObject.GetComponent<ICardDropTarget>();
                if (dropTarget != null && dropTarget.CanDrop(this))
                {
                    dropTarget.DropCardAfterDrag(this);
                    return;
                }
            }

            tempPickupTarget?.ReturnCardOnNoTarget(this);
            tempPickupTarget = null;
        }

        // =========================================================
        // RETURN TO ORIGINAL POSITION
        // =========================================================

        // public void ReturnToHand()
        // {
        //     transform.SetParent(_originalParent);
        //     transform.SetSiblingIndex(_originalParent.childCount - 1);

        //     _rectTransform.anchoredPosition =
        //         _originalAnchoredPosition;
        // }

        public void DisableInteraction()
        {
            wasInteractionEnabledBeforeDrag = _canvasGroup.blocksRaycasts;
            _canvasGroup.blocksRaycasts = false;
        }
        void ResumeInteraction()
        {
            _canvasGroup.blocksRaycasts = wasInteractionEnabledBeforeDrag;
        }
        public void EnableInteraction()
        {
            _canvasGroup.blocksRaycasts = true;
        }
        public void ToggleInteractionType()
        {
            interactionType = (interactionType == InteractionType.Drag) ? InteractionType.Click : InteractionType.Drag;
        }
        public void ToggleBackFace(bool showBack)
        {
            backFaceImage.gameObject.SetActive(showBack);
        }
        public void ScaleTo(float value) => _rectTransform.DOScale(value, TIME_TO_SCALE_IN_SECS);
        public void SetAsLastSibbling() => transform.SetSiblingIndex(transform.parent.childCount - 1);

        public float GetWidth() => _rectTransform.sizeDelta.x;

        public void ActivateGlow()
        {
            glowBG.ShowEffect();
        }

        public void DeactivateGlow()
        {
            glowBG.Reset();
        }
    }
    public enum InteractionType
    {
        Drag,
        Click
    }
}