using System.Collections.Generic;
using UnityEngine;

public class RouletteTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(4f, 2f, 1f);

    private CrowdSystem crowdSystem;
    private bool crowdDetected;

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        DetectCrowd();
    }

    private void DetectCrowd()
    {
        if (crowdDetected)
        {
            return;
        }

        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = detectionBoxSize * 0.5f;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 runnerPosition = runners[i].transform.position - transform.position;

            bool isInside = Mathf.Abs(runnerPosition.x) <= halfBoxSize.x
                && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y
                && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z;

            if (isInside)
            {
                crowdDetected = true;
                GameManager.Instance.SetGameStatus(GameStatus.Roullete);
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, detectionBoxSize);
    }
}
