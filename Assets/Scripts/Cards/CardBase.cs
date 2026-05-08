
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TacoVsBurrito
{
    public abstract class CardBase: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Identity")]
        [SerializeField] string cardName = "Unnamed Card";
        [SerializeField]  string DescriptionText = "";
        [SerializeField]  bool isPlaceableInMeal = false;
        [SerializeField] bool isBlockable = false;

        [Header("Fields")]
        [SerializeField] protected TextMeshProUGUI nameTxtField;
        [SerializeField] protected TextMeshProUGUI DescriptionTxtField;


        [Header("Drag Settings")]
        [SerializeField] private float dragScale = 1.2f;

        private RectTransform _rectTransform;
        private Canvas canvas;
        private CanvasGroup _canvasGroup;

        private Transform _originalParent;
        private int _originalSiblingIndex;

        private Vector2 _originalAnchoredPosition;
        private Vector3 _originalScale;

        public string Name { get { return cardName; } }

        // Common helpers
        public virtual bool IsPlaceableInMeal => isPlaceableInMeal;
        public virtual bool IsBlockable => isBlockable;

        public virtual int GetModifiedMealScore(int currentScore) { return currentScore; }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            canvas = GetComponentInParent<Canvas>();

            GameEvents.OnCardDragBegin += DisableInteraction;
            GameEvents.OnCardDragEnd += EnableInteraction;
        }

        void OnDestroy()
        {
            GameEvents.OnCardDragBegin -= DisableInteraction;
            GameEvents.OnCardDragEnd -= EnableInteraction;
        }

        protected virtual void Start()
        {
            nameTxtField.text = cardName;
            DescriptionTxtField.text = DescriptionText;
        }

        public void ChangePosition(Vector3 newPos)
        {
            transform.DOMove(newPos, 0.5f);
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
        // BEGIN DRAG
        // =========================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            GameEvents.OnCardDragBegin();

            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();

            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _originalScale = transform.localScale;

            // Move outside layout group while dragging
            transform.SetParent(canvas.transform);

            // Bring on top
            transform.SetAsLastSibling();

            // Allow raycasts to pass through while dragging
            _canvasGroup.blocksRaycasts = false;

            // Optional visual feedback
            transform.localScale = Vector3.one * dragScale;

            StartCoroutine(PickCardBeforeDrag(eventData));

        }

        IEnumerator PickCardBeforeDrag(PointerEventData eventData)
        {
            yield return null;
            GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;
            if (targetObject != null)
            {
                ICardPickupTarget pickupTarget = targetObject.GetComponent<ICardPickupTarget>();
                Debug.Log(pickupTarget == null);
                pickupTarget?.PickCardBeforeDrag(this);
            }
        }
        // =========================================================
        // DRAG
        // =========================================================

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition +=
                eventData.delta / canvas.scaleFactor;
        }

        // =========================================================
        // END DRAG
        // =========================================================

        public void OnEndDrag(PointerEventData eventData)
        {
            GameEvents.OnCardDragEnd();
            _canvasGroup.blocksRaycasts = true;

            transform.localScale = _originalScale;

            GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;
            if (targetObject != null)
            {
                ICardDropTarget dropTarget = targetObject.GetComponent<ICardDropTarget>();
                if (dropTarget != null && dropTarget.CanDrop(this))
                {
                    dropTarget.DropCardAfterDrag(this);
                    return;
                }
            }
            ReturnToHand();
        }

        // =========================================================
        // RETURN TO ORIGINAL POSITION
        // =========================================================

        public void ReturnToHand()
        {
            transform.SetParent(_originalParent);
            transform.SetSiblingIndex(_originalSiblingIndex);

            _rectTransform.anchoredPosition =
                _originalAnchoredPosition;
        }

        public void DisableInteraction()
        {
            _canvasGroup.blocksRaycasts = false;
        }
        public void EnableInteraction()
        {
            _canvasGroup.blocksRaycasts = true;
        }
    }
}