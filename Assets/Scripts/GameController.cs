using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private PathRenderer pathRenderer;
    [SerializeField] private Marker marker;
    [SerializeField] private CardsUI cardsUI;
    [SerializeField] private List<CardScriptableObject> initialCardsSet;
    [SerializeField] private int initialCardsCount = 6;
    [SerializeField] private List<CardScriptableObject> allCards;
    [SerializeField] private CategoriesSettings categoriesSettings;
    [SerializeField] private SoundSystem soundSystem;
    [SerializeField] private AudioClip soundtrack;
    [SerializeField] private Button startGameButton;
    [SerializeField] private CanvasGroup startGameAnimation;
    [SerializeField] private CanvasGroup endGameAnimation;
    [SerializeField] private AudioClip collectSfx;
    [SerializeField] private float pitchCooldown = 0.4f;
    [SerializeField] private float pitchStep = 0.2f;
    [SerializeField] private float maxPitch = 2f;

    private int _currentPointIndex;
    private bool _gameplayStarted;
    private float _endGameCountdown = 2f;
    private float _pitchCooldown = 0.2f;
    private float _currentPitch = 1f;

    public static GameController Instance { get; private set; }

    public readonly EventsDispatcher eventsDispatcher = new ();

    public PathRenderer PathRenderer => pathRenderer;
    public SoundSystem SoundSystem => soundSystem;

    public CardScriptableObject GetRandomCardOfType(ScoreType type)
    {
        var selection = allCards.Where(c => c.type == type);
        return selection.RandomElement();
    }

    public Tag GetRandomTagByScoreType(ScoreType scoreType)
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
        soundSystem.PlayMusicClip(soundtrack);
        
        startGameButton.onClick.AddListener(() => StartGameRoutine().Forget());
        
        GetComponent<CollectablesGenerator>().Initialize();
    }

    private void StartGameplay()
    {
        _currentPointIndex = 0;
        marker.gameObject.transform.position = pathRenderer.GetPointPosition(_currentPointIndex);

        GenerateInitialCards();
        
        eventsDispatcher.Register<CardApplyEvent>(this, OnCardApplied);
        eventsDispatcher.Register<PlayCollectSfxEvent>(this, OnPlayCollectSfxEvent);

        _gameplayStarted = true;
    }

    private void OnPlayCollectSfxEvent(PlayCollectSfxEvent e)
    {
        if (_pitchCooldown <= 0f)
        {
            _currentPitch = 1f;
        }
        else
        {
            _currentPitch += pitchStep;
        }

        if (_currentPitch > maxPitch)
        {
            _currentPitch = maxPitch;
        }
        
        _pitchCooldown = pitchCooldown;
        
        soundSystem.PlayOneShot(collectSfx, _currentPitch);
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
        _pitchCooldown -= Time.deltaTime;
        
        if (!_gameplayStarted)
        {
            return;
        }

        if (_endGameCountdown <= 0f)
        {
            EndGameRoutine().Forget();
            enabled = false;
            return;
        }

        if (_currentPointIndex >= pathRenderer.LastPlannedPointIndex)
        {
            if (cardsUI.CurrentCardsCount == 0)
            {
                _endGameCountdown -= Time.deltaTime;
            }
            
            return;
        }

        _endGameCountdown = 2f;
        MoveMarker(marker.Speed * Time.deltaTime);
    }
    
    private async UniTaskVoid StartGameRoutine()
    {
        startGameButton.interactable = false;
        startGameAnimation.DOFade(0f, 0.5f);
        
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        
        startGameAnimation.gameObject.SetActive(false);
        
        StartGameplay();
    }

    private async UniTaskVoid EndGameRoutine()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        SceneManager.LoadScene(0, LoadSceneMode.Single);
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
        if (_currentPointIndex >= pathRenderer.LastPlannedPointIndex)
        {
            return;
        }
        
        MoveMarker(Mathf.Max(0f, moveDistance));
    }

    private void OnDestroy()
    {
        eventsDispatcher.Unregister<CardApplyEvent>(this);
        eventsDispatcher.Unregister<PlayCollectSfxEvent>(this);
    }
}
