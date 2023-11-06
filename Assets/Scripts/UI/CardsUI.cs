using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace UI
{
    public class CardsUI : MonoBehaviour, IEventsDispatcherClient
    {
        [SerializeField] private Transform cardsTransform;
        [SerializeField] private UiCard uiCardPrefab;

        private readonly CompositeDisposable _compositeDisposable = new ();
        private readonly List<UiCard> _uiCards;

        public int CurrentCardsCount => cardsTransform.childCount;

        private void Start()
        {
            GameController.Instance.eventsDispatcher.Register<AddCardEvent>(this, OnCardAdded);
        }

        public void Initialize(List<Card> startCards)
        {
            foreach (var startCard in startCards)
            {
                var uiCard = Instantiate(uiCardPrefab, cardsTransform);
                uiCard.SetCard(startCard);
            }
        }

        private void OnCardAdded(AddCardEvent addEvent)
        {
            var card = new Card(GameController.Instance.GetRandomCardOfType(), addEvent.scoreType);
            var uiCard = Instantiate(uiCardPrefab, cardsTransform);
            uiCard.SetCard(card);
        }

        private void OnDestroy()
        {
            _compositeDisposable.Dispose();
        }
    }
}
