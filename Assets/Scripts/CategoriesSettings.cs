using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CategoriesSettings : ScriptableObject
{
    public CategorySettings Red;
    public CategorySettings Green;
    public CategorySettings Blue;
    public CategorySettings Negative;
}

[Serializable]
public class CategorySettings
{
    public List<Tag> Tags;
}
