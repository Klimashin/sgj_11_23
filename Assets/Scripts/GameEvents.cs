
using UnityEngine;

public record AddScoreEvent : IDispatcherEvent
{
    public readonly int Score;
    public readonly ScoreType ScoreType;

    public AddScoreEvent(int score, ScoreType scoreType)
    {
        Score = score;
        ScoreType = scoreType;
    }
}

public record AddNegativeTraitEvent : IDispatcherEvent;

public record GameResetEvent : IDispatcherEvent;

public record AddCardEvent : IDispatcherEvent
{
    public readonly ScoreType scoreType;

    public AddCardEvent(ScoreType scoreType)
    {
        this.scoreType = scoreType;
    }
}

public record CardDragStarted : IDispatcherEvent
{
    public readonly LineRenderer segmentLineRenderer;

    public CardDragStarted(LineRenderer lineRenderer)
    {
        segmentLineRenderer = lineRenderer;
    }
}

public record CardDragEnded : IDispatcherEvent;

public record CardApplyEvent : IDispatcherEvent
{
    public readonly Card card;
    public readonly Vector2 screenPoint;

    public CardApplyEvent(Card card, Vector2 screenPoint)
    {
        this.card = card;
        this.screenPoint = screenPoint;
    }
}

public record ShowComboEvent : IDispatcherEvent
{
    public readonly int comboCounter;
    public readonly ScoreType scoreType;

    public ShowComboEvent(int comboCounter, ScoreType scoreType)
    {
        this.comboCounter = comboCounter;
        this.scoreType = scoreType;
    }
}
