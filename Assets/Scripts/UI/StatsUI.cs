using System.Linq;
using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private StatsController statsController;
    [SerializeField] private float updateCooldown = 1f;
    [SerializeField] private RectTransform achievementsTransform;
    [SerializeField] private RectTransform tagsTransform;
    [SerializeField] private TextMeshProUGUI tagTextPrefab;
    [SerializeField] private TextMeshProUGUI achievementTextPrefab;

    private float _currentCooldown;
    
    private void Update()
    {
        _currentCooldown -= Time.deltaTime;
        if (_currentCooldown <= 0f)
        {
            _currentCooldown = updateCooldown;
            UpdateView();
        }
    }

    private void UpdateView()
    {
        var stats = statsController.GetStats();

        for (int i = tagsTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(tagsTransform.GetChild(i).gameObject);
        }

        var tags = stats.CollectedTags.GroupBy(t => t.Name);
        foreach (var grouping in tags)
        {
            var text = $"{grouping.First().Name} x{grouping.Count()}";
            var textUi = Instantiate(tagTextPrefab, tagsTransform);
            textUi.text = text;
        }
        
        for (int i = achievementsTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(achievementsTransform.GetChild(i).gameObject);
        }

        var achievements = stats.CollectedAchievements.GroupBy(a => a.Name);
        foreach (var grouping in achievements)
        {
            var text = $"{grouping.First().Name} x{grouping.Count()}";
            var textUi = Instantiate(achievementTextPrefab, achievementsTransform);
            textUi.text = text;
        }
    }
}
