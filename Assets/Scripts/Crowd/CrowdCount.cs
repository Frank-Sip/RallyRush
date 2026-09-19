using UnityEngine;
using TMPro;

public class CrowdCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crowdCountText;

    private CrowdSystem crowdSystem;

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        UpdateCrowdCount();
    }

    private void UpdateCrowdCount()
    {
        crowdCountText.text = crowdSystem.GetRunnersAmount().ToString();
    }
}
