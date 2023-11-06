using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AchievementsUi : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private CanvasGroup achievementPanel;
    [SerializeField] private TextMeshProUGUI achievementText;
    [SerializeField] private CanvasGroup negativeTraitPanel;
    [SerializeField] private TextMeshProUGUI negativeTraitText;
    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private float displayTime = 1f;

    private readonly Queue<(string, bool)> _notificationsQueue = new ();
    private bool _isPlayingAnimation = false;
    
    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<ShowAchievementEvent>(this, OnShowAchievement);
        GameController.Instance.eventsDispatcher.Register<ShowNegativeTagEvent>(this, OnShowNegativeTag);
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

        var notiData = _notificationsQueue.Dequeue();
        ShowNotification(notiData.Item1, notiData.Item2).Forget();
    }

    private async UniTaskVoid ShowNotification(string text, bool isNegative)
    {
        var panel = isNegative ? negativeTraitPanel : achievementPanel;
        var textUi = isNegative ? negativeTraitText : achievementText;
        
        panel.alpha = 0f;
        var fadeInTween = panel.GetComponent<CanvasGroup>().DOFade(1f, transitionTime);
        while (fadeInTween.IsPlaying())
        {
            await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
        }

        panel.transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f, 1);
        await UniTask.Delay(TimeSpan.FromSeconds(displayTime), cancellationToken: destroyCancellationToken);
        
        var fadeOutTween = panel.GetComponent<CanvasGroup>().DOFade(0f, transitionTime);
        while (fadeOutTween.IsPlaying())
        {
            await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
        }
        
        _isPlayingAnimation = false;
    }

    private void OnShowNegativeTag(ShowNegativeTagEvent showNegativeTagEvent)
    {
        
    }

    private void OnShowAchievement(ShowAchievementEvent showAchievementEvent)
    {
        throw new System.NotImplementedException();
    }

    private void OnDestroy()
    {
        GameController.Instance.eventsDispatcher.Unregister<ShowAchievementEvent>(this);
        GameController.Instance.eventsDispatcher.Unregister<ShowNegativeTagEvent>(this);
    }
}
