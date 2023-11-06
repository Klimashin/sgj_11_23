using System.Collections.Generic;
using UnityEngine;

public class StatsController : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private List<Achievement> achievements;

    private readonly List<Tag> _allCollectedTags = new ();
    private readonly List<Achievement> _collectedAchievements = new ();
    private List<Tag> _unusedCollectedTags = new ();

    public Stats GetStats()
    {
        return new (_allCollectedTags, _collectedAchievements);
    }

    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<CardApplyEvent>(this, OnCardApplied);
        GameController.Instance.eventsDispatcher.Register<AddNegativeTraitEvent>(this, OnNegativeTraitAdded);
    }

    private void OnNegativeTraitAdded(AddNegativeTraitEvent obj)
    {
        var negativeTag = GameController.Instance.GetRandomTagByScoreType(ScoreType.Negative);
        _allCollectedTags.Add(negativeTag);
        _unusedCollectedTags.Add(negativeTag);
        CheckForAchievements();
    }

    private ScoreType _lastAppliedScoreType = ScoreType.Negative;
    private int _comboCounter;
    private void OnCardApplied(CardApplyEvent cardApplyEvent)
    {
        _allCollectedTags.Add(cardApplyEvent.card.GetTag());
        _unusedCollectedTags.Add(cardApplyEvent.card.GetTag());
        CheckForAchievements();
        
        if (cardApplyEvent.card.ScoreType == _lastAppliedScoreType)
        {
            _comboCounter++;
        }
        else
        {
            _comboCounter = 1;
        }

        _lastAppliedScoreType = cardApplyEvent.card.ScoreType;
        if (_comboCounter >= 2)
        {
            GameController.Instance.eventsDispatcher.Dispatch(new ShowComboEvent(_comboCounter, _lastAppliedScoreType));
        }
    }

    private void CheckForAchievements()
    {
        foreach (var achievement in achievements)
        {
            List<Tag> availableTags = new List<Tag>(_unusedCollectedTags);
            int requirementsMet = 0;
            foreach (var achievementRequirement in achievement.Requirements)
            {
                bool isMet = _unusedCollectedTags.FindAll(t => t == achievementRequirement.tag).Count >=
                             achievementRequirement.count;
                if (isMet)
                    requirementsMet += 1;
            }

            int minReqToComplete = achievement.MinRequirementsToComplete > 0
                ? achievement.MinRequirementsToComplete
                : achievement.Requirements.Count;
            if (requirementsMet >= minReqToComplete)
            {
                foreach (var achievementRequirement in achievement.Requirements)
                {
                    for (int i = 0; i < achievementRequirement.count; i++)
                    {
                        availableTags.Remove(achievementRequirement.tag);
                    }
                }
                
                _collectedAchievements.Add(achievement);
                GameController.Instance.eventsDispatcher.Dispatch(new ShowComboEvent(_comboCounter, _lastAppliedScoreType));

                _unusedCollectedTags = availableTags;
            }
        }
    }
    
    private void OnDestroy()
    {
        GameController.Instance.eventsDispatcher.Unregister<CardApplyEvent>(this);
        GameController.Instance.eventsDispatcher.Unregister<AddNegativeTraitEvent>(this);
    }
}

public record Stats
{
    public readonly List<Tag> CollectedTags;
    public readonly List<Achievement> CollectedAchievements;

    public Stats(List<Tag> tags, List<Achievement> achievements)
    {
        CollectedTags = tags;
        CollectedAchievements = achievements;
    }
}
