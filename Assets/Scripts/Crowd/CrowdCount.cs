using TMPro;
using UnityEngine;

public class CrowdCount
{
    private readonly TextMeshProUGUI crowdCountText;
    private readonly GameObject crowdCountUI;
    private CrowdSystem crowdSystem;

    public CrowdCount(TextMeshProUGUI text)
    {
        crowdCountText = text;
        crowdCountUI = crowdCountText.canvas.gameObject;
    }

    public void CustomAwake()
    {
        ServiceProvider.RegisterService(this);
    }

    public void CustomStart()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
        crowdSystem.RunnersAmountChanged += UpdateCrowdCount;
        UpdateCrowdCount(crowdSystem.GetRunnersAmount());
    }

    public void CustomDestroy()
    {
        crowdSystem.RunnersAmountChanged -= UpdateCrowdCount;
    }

    public void SetActive(bool isActive)
    {
        crowdCountUI.SetActive(isActive);
    }

    private void UpdateCrowdCount(int amount)
    {
        crowdCountText.text = amount.ToString();
    }
}
