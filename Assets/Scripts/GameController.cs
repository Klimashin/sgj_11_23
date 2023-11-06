using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class GameController : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private PathRenderer pathRenderer;
    [SerializeField] private Marker marker;
    [SerializeField] private CardsUI cardsUI;
    [SerializeField] private List<CardScriptableObject> initialCardsSet;
    [SerializeField] private int initialCardsCount = 6;
    [SerializeField] private List<CardScriptableObject> allCards;
    [SerializeField] private CategoriesSettings categoriesSettings;

    private int _currentPointIndex;
    private bool _initialized;

    public static GameController Instance { get; private set; }

    public readonly EventsDispatcher eventsDispatcher = new ();

    public PathRenderer PathRenderer => pathRenderer;

    public CardScriptableObject GetRandomCardOfType(ScoreType type)
    {
        var selection = allCards.Where(c => c.type == type);
        return selection.RandomElement();
    }

    public string GetRandomTagByScoreType(ScoreType scoreType)
    {
        switch (scoreType)
        {
            case ScoreType.Red:
                return categoriesSettings.Red.Tags.RandomElement();
            
            case ScoreType.Green:
                return categoriesSettings.Green.Tags.RandomElement();
            
            case ScoreType.Blue:
                return categoriesSettings.Blue.Tags.RandomElement();
            
            case ScoreType.Negative:
                return categoriesSettings.Negative.Tags.RandomElement();
            
            default:
                throw new Exception("Undefined category");
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("GameController Instance is not null");
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    private void Start()
    {
        _currentPointIndex = 0;
        marker.gameObject.transform.position = pathRenderer.GetPointPosition(_currentPointIndex);
        
        GetComponent<CollectablesGenerator>().Initialize();

        GenerateInitialCards();
        
        eventsDispatcher.Register<CardApplyEvent>(this, OnCardApplied);

        _initialized = true;
    }

    private void OnCardApplied(CardApplyEvent cardApplyEvent)
    {
        var segmentPosCount = cardApplyEvent.card.PathSegment.positionCount;
        var segmentData = new Vector3[segmentPosCount];
        cardApplyEvent.card.PathSegment.GetPositions(segmentData);
        pathRenderer.AddPathSegment(segmentData);
    }

    private void GenerateInitialCards()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < initialCardsCount; i++)
        {
            var cardSo = initialCardsSet.RandomElement();
            cards.Add(new Card(cardSo));
        }
        
        cardsUI.Initialize(cards);
    }

    private void Update()
    {
        if (!_initialized || _currentPointIndex >= pathRenderer.LastPointIndex)
        {
            return;
        }

        MoveMarker(marker.Speed * Time.deltaTime);
    }

    private void MoveMarker(float moveDistance)
    {
        Vector3 currentPosition = marker.transform.position;
        Vector3 targetPosition = pathRenderer.GetPointPosition(_currentPointIndex + 1);
        float distance = Vector3.Distance(currentPosition, targetPosition);

        if (distance >= moveDistance)
        {
            Vector3 directionOfTravel = targetPosition - currentPosition;
            directionOfTravel.Normalize();
            marker.transform.Translate(
                directionOfTravel.x * moveDistance, 
                directionOfTravel.y * moveDistance, 
                directionOfTravel.z * moveDistance, 
                Space.World);
            
            float angle = Mathf.Atan2(directionOfTravel.y, directionOfTravel.x) * Mathf.Rad2Deg - 90f;
            marker.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            return;
        }
        
        marker.transform.position = targetPosition;
        moveDistance -= distance;
        _currentPointIndex++;
        if (_currentPointIndex >= pathRenderer.LastPointIndex)
        {
            return;
        }
        
        MoveMarker(Mathf.Max(0f, moveDistance));
    }

    private void OnDestroy()
    {
        eventsDispatcher.Unregister<CardApplyEvent>(this);
    }
}
