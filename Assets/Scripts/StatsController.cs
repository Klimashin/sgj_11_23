using UnityEngine;

public class StatsController : MonoBehaviour, IEventsDispatcherClient
{
    private GameController _gameController;
    private GameController GameController => _gameController ??= GetComponent<GameController>();

    private void Start()
    {
        GameController.eventsDispatcher.Register<CardApplyEvent>(this, OnCardApplied);
    }

    private void OnDestroy()
    {
        GameController.eventsDispatcher.Unregister<CardApplyEvent>(this);
    }

    private ScoreType _lastAppliedScoreType = ScoreType.Negative;
    private int _comboCounter;
    private void OnCardApplied(CardApplyEvent cardApplyEvent)
    {
        if (cardApplyEvent.card.ScoreType == _lastAppliedScoreType)
        {
            _comboCounter++;
        }
        else
        {
            _comboCounter = 1;
        }

        _lastAppliedScoreType = cardApplyEvent.card.ScoreType;
        GameController.eventsDispatcher.Dispatch(new ShowComboEvent(_comboCounter, _lastAppliedScoreType));
    }
}
