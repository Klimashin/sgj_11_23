using UnityEngine;

public class Card
{
    private readonly CardScriptableObject _scriptableObject;

    public Sprite UiSprite => _scriptableObject.uiSprite;
    public LineRenderer PathSegment => _scriptableObject.pathSegment;
    public ScoreType ScoreType => _scriptableObject.type;
    public readonly string Name;
    
    public Card(CardScriptableObject scriptableObject)
    {
        _scriptableObject = scriptableObject;
        Name = GameController.Instance.GetRandomTagByScoreType(ScoreType);
    }
}
