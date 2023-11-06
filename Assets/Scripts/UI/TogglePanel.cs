using UnityEngine;
using UnityEngine.UI;

public class TogglePanel : MonoBehaviour
{
    [SerializeField] private GameObject cardsUi;
    [SerializeField] private GameObject statsUi;
    [SerializeField] private Button showStatsButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        showStatsButton.onClick.AddListener(() =>
        {
            showStatsButton.gameObject.SetActive(false);
            cardsUi.gameObject.SetActive(false);
            backButton.gameObject.SetActive(true);
            statsUi.gameObject.SetActive(true);
        });
        
        backButton.onClick.AddListener(() =>
        {
            showStatsButton.gameObject.SetActive(true);
            cardsUi.gameObject.SetActive(true);
            backButton.gameObject.SetActive(false);
            statsUi.gameObject.SetActive(false);
        });
    }
}
