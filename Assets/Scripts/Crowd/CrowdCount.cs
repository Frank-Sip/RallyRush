using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrowdCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crowdCountText;
    [SerializeField] private Transform playerTransform;

    private readonly List<Runner> runners = new List<Runner>();

    private void Update()
    {
        UpdateCrowdCount();
    }

    private void UpdateCrowdCount()
    {
        runners.Clear();
        playerTransform.GetComponentsInChildren(false, runners);
        crowdCountText.text = runners.Count.ToString();
    }
}
