using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Tag : ScriptableObject
{
    [SerializeField] private string tagName;
    [SerializeField] private List<string> metaTags;

    public string Name => tagName;
    public List<string> MetaTags => metaTags;
}
