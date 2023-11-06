
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Achievement : ScriptableObject
{
    public string Name;
    public List<AchievementRequirement> Requirements;
    public int MinRequirementsToComplete = 0;
}

[Serializable]
public class AchievementRequirement
{
    public Tag tag;
    public int count = 1;
}
