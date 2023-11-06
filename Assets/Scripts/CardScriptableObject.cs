using UnityEngine;

[CreateAssetMenu]
public class CardScriptableObject : ScriptableObject
{
    [SerializeField] public LineRenderer pathSegment;
    [SerializeField] public Sprite uiSprite;
    [SerializeField] public ScoreType type;
}
