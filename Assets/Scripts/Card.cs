using UnityEngine;

public class Card
{
    private readonly CardScriptableObject _scriptableObject;
    private readonly Tag _tag;
    private readonly ScoreType _scoreType;

    public Sprite UiSprite => _scriptableObject.uiSprite;
    public LineRenderer PathSegment => _scriptableObject.pathSegment;
    public ScoreType ScoreType => _scoreType;
    public readonly string Name;
    
    public Card(CardScriptableObject scriptableObject, ScoreType scoreType)
    {
        _scoreType = scoreType;
        _scriptableObject = scriptableObject;
        _tag = GameController.Instance.GetRandomTagByScoreType(scoreType);
        Name = _tag.Name;
    }

    public Tag GetTag() => _tag;
}
