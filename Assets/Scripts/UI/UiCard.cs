using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UiCard : MonoBehaviour, IUiDraggable
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private Image raycastTarget;
        [SerializeField] private TextMeshProUGUI cardTitle;
        [SerializeField] private Sprite[] bgSprites;

        private Card _card;
        private Vector3 _intitialPos;

        public void SetCard(Card card)
        {
            _card = card;
            cardImage.sprite = _card.UiSprite;
            cardTitle.text = _card.Name;
            raycastTarget.sprite = bgSprites[(int) _card.ScoreType];
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _intitialPos = transform.position;
            raycastTarget.raycastTarget = false;
            GameController.Instance.eventsDispatcher.Dispatch(new CardDragStarted(_card.PathSegment));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            raycastTarget.raycastTarget = true;
            
            GameController.Instance.eventsDispatcher.Dispatch(new CardDragEnded());
            
            List<RaycastResult> raycastResults = new ();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            
            foreach (var raycastResult in raycastResults)
            {
                if (raycastResult.gameObject.CompareTag($"CardsDrop"))
                {
                    GameController.Instance.eventsDispatcher.Dispatch(new CardApplyEvent(_card, eventData.position));
                    Destroy(gameObject);
                    return;
                }
            }

            transform.position = _intitialPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            GetComponent<RectTransform>().position = eventData.position;
        }
    }
}
