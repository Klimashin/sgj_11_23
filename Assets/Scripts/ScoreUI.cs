using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private Image[] scorePointImages;
    [SerializeField] private ScoreType scoreType;
    [SerializeField] private Color pointActiveColor;
    [SerializeField] private Color pointInactiveColor;

    private int _currentScore;
    private CancellationTokenSource _cancellationTokenSource;

    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<AddScoreEvent>(this, OnAddScore);
    }

    private void OnDestroy()
    {
        GameController.Instance.eventsDispatcher.Unregister<AddScoreEvent>(this);
    }

    private void OnAddScore(AddScoreEvent addScoreEvent)
    {
        if (addScoreEvent.ScoreType == scoreType)
        {
            _currentScore += addScoreEvent.Score;
            for (int i = 0; i < scorePointImages.Length; i++)
            {
                scorePointImages[i].color = _currentScore > i ? pointActiveColor : pointInactiveColor;
            }

            if (_currentScore >= scorePointImages.Length)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                scorePointImages[^1].color = pointActiveColor;
                HandleReceiveTag(_cancellationTokenSource.Token).Forget();
            }
        }
    }

    private async UniTaskVoid HandleReceiveTag(CancellationToken cancellationToken)
    {
        try
        {
            _currentScore = 0;
            
            await UniTask.WaitForSeconds(1f, true, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            if (this == null || GameController.Instance == null)
            {
                return;
            }
            
            GameController.Instance.eventsDispatcher.Dispatch(new AddTagEvent(transform.position));
            GameController.Instance.eventsDispatcher.Dispatch(new AddCardEvent(scoreType, transform.position));
            Console.WriteLine(e);
            return;
        }

        for (int i = 0; i < scorePointImages.Length; i++)
        {
            scorePointImages[i].color = _currentScore > i ? pointActiveColor : pointInactiveColor;
        }

        GameController.Instance.eventsDispatcher.Dispatch(new AddTagEvent(transform.position));
        GameController.Instance.eventsDispatcher.Dispatch(new AddCardEvent(scoreType, transform.position));
    }
}
