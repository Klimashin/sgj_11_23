using UnityEngine;

[CreateAssetMenu]
public class Tag : ScriptableObject
{
    [SerializeField] private string tagName;
    [SerializeField] private ScoreType type;

    public string Name => tagName;
    public ScoreType Type => type;
}
