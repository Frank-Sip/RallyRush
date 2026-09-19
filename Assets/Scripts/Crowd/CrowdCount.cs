using UnityEngine;
using TMPro;

public class CrowdCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crowdCountText;

    private CrowdSystem crowdSystem;

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        crowdSystem.RunnersAmountChanged += UpdateCrowdCount;
        UpdateCrowdCount(crowdSystem.GetRunnersAmount());
    }

    private void OnDestroy()
    {
        if (crowdSystem != null)
        {
            crowdSystem.RunnersAmountChanged -= UpdateCrowdCount;
        }
    }

    private void UpdateCrowdCount(int amount)
    {
        crowdCountText.text = amount.ToString();
    }
}
