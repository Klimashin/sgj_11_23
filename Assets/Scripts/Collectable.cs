using DG.Tweening;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private ScoreType scoreType;

    private Collider2D _collider2D;
    private Collider2D Collider2D => _collider2D ??= GetComponentInChildren<Collider2D>();
    private const float SCALE_DURATION = 0.3f;

    public ScoreType ScoreType => scoreType;

    public CollectablesGenerator.CollectableSettings SettingsRef { get; set; }

    public void Collect()
    {
        Collider2D.enabled = false;

        GameController.Instance.eventsDispatcher.Dispatch(scoreType != ScoreType.Negative ? new PlayCollectSfxEvent() : new PlayBadBallSfxEvent());
        
        transform
            .DOScale(Vector3.zero, SCALE_DURATION)
            .OnComplete(() =>
            {
                if (scoreType != ScoreType.Negative)
                {
                    GameController.Instance.eventsDispatcher.Dispatch(new AddScoreEvent(1, scoreType));
                }
                else
                {
                    GameController.Instance.eventsDispatcher.Dispatch(new AddNegativeTraitEvent());
                }
            });
    }

    public void OnReturnedToPool()
    {
        
    }

    public void OnTakeFromPool()
    {
        transform.localScale = Vector3.one;
        Collider2D.enabled = true;
    }
}