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
    [SerializeField] private string redCategoryText;
    [SerializeField] private Color greenCategoryColor;
    [SerializeField] private string greenCategoryText;
    [SerializeField] private Color blueCategoryColor;
    [SerializeField] private string blueCategoryText;
    [SerializeField] private float scaleUpDuration;
    [SerializeField] private float scaleDownDuration;

    private CancellationTokenSource _cancellationTokenSource;
    
    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<ShowComboEvent>(this, OnShowCombo);
    }

    private void OnShowCombo(ShowComboEvent comboEvent)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        
        switch (comboEvent.scoreType)
        {
            case ScoreType.Red:
                notificationText.text = redCategoryText + $" x{comboEvent.comboCounter.ToString()}";
                notificationText.color = redCategoryColor;
                break;
            
            case ScoreType.Green:
                notificationText.text = greenCategoryText + $" x{comboEvent.comboCounter.ToString()}";
                notificationText.color = greenCategoryColor;
                break;
            
            case ScoreType.Blue:
                notificationText.text = blueCategoryText + $" x{comboEvent.comboCounter.ToString()}";
                notificationText.color = blueCategoryColor;
                break;
        }

        ShowComboTextAnimation(comboEvent, _cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid ShowComboTextAnimation(ShowComboEvent comboEvent, CancellationToken cancellationToken)
    {
        try
        {
            notificationTransform.gameObject.SetActive(true);

            var scaleUpTween = notificationTransform.DOScale(Vector3.one * (1 + comboEvent.comboCounter * 0.2f), scaleUpDuration);
            while (scaleUpTween.IsPlaying())
            {
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            }
            
            var scaleDownTween = notificationTransform.DOScale(Vector3.one, scaleDownDuration);
            while (scaleDownTween.IsPlaying())
            {
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
            }
        }
        finally
        {
            notificationTransform.gameObject.SetActive(false);
            notificationTransform.localScale = Vector3.one;
            
            if (comboEvent.comboCounter == 3)
            {
                GameController.Instance.eventsDispatcher.Dispatch(new AddScoreEvent(1, comboEvent.scoreType));
            }
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        GameController.Instance.eventsDispatcher.Unregister<ShowComboEvent>(this);
    }
}
