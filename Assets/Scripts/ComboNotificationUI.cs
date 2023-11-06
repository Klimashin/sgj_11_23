using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboNotificationUI : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private RectTransform notificationTransform;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Color redCategoryColor;
    [SerializeField] private Color greenCategoryColor;
    [SerializeField] private Color blueCategoryColor;
    [SerializeField] private float scaleUpDuration;
    [SerializeField] private float scaleDownDuration;

    private CancellationTokenSource _cancellationTokenSource;
    
    private readonly Queue<ShowComboEvent> _notificationsQueue = new ();
    private bool _isPlayingAnimation = false;
    
    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<ShowComboEvent>(this, OnShowCombo);
    }
    
    private void Update()
    {
        if (_isPlayingAnimation)
        {
            return;
        }

        if (_notificationsQueue.Count == 0)
        {
            return;
        }
        
        _isPlayingAnimation = true;
        ShowCombo(_notificationsQueue.Dequeue()).Forget();
    }

    private async UniTaskVoid ShowCombo(ShowComboEvent comboEvent)
    {
        notificationText.text = comboEvent.comboText;

        switch (comboEvent.scoreType)
        {
            case ScoreType.Red:
                notificationText.color = redCategoryColor;
                break;
            
            case ScoreType.Green:
                notificationText.color = greenCategoryColor;
                break;
            
            case ScoreType.Blue:
                notificationText.color = blueCategoryColor;
                break;
        }
        
        notificationTransform.gameObject.SetActive(true);

        var scaleUpTween = notificationTransform.DOScale(Vector3.one * (1 + comboEvent.comboCounter * 0.2f), scaleUpDuration);
        while (scaleUpTween.IsPlaying())
        {
            await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
        }
            
        var scaleDownTween = notificationTransform.DOScale(Vector3.one, scaleDownDuration);
        while (scaleDownTween.IsPlaying())
        {
            await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
        }
        
        GameController.Instance.eventsDispatcher.Dispatch(new AddScoreEvent(1, comboEvent.scoreType));
        
        notificationTransform.gameObject.SetActive(false);
        _isPlayingAnimation = false;
    }

    private void OnShowCombo(ShowComboEvent comboEvent)
    {
        _notificationsQueue.Enqueue(comboEvent);
    }

    private void OnDestroy()
    {
        GameController.Instance.eventsDispatcher.Unregister<ShowComboEvent>(this);
    }
}
