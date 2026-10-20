using System.Collections.Generic;
using UnityEngine;

public class Ramp : MonoBehaviour
{
    [Min(0f)] [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Vector3 detectionBoxSize = new Vector3(4f, 2f, 1f);

    [Header("Audio")]
    [Min(0)] [SerializeField] private int jumpSfxIndex;

    private readonly HashSet<Runner> runnersInside = new HashSet<Runner>();
    private CrowdSystem crowdSystem;
    private bool jumpSfxPlayed;

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        DetectRunners();
    }

    private void DetectRunners()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = detectionBoxSize * 0.5f;

        for (int i = 0; i < runners.Count; i++)
        {
            Runner runner = runners[i];
            Vector3 runnerPosition = runner.transform.position - transform.position;

            bool isInside = Mathf.Abs(runnerPosition.x) <= halfBoxSize.x && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z;

            if (isInside && runnersInside.Add(runner))
            {
                runner.Jump(jumpForce);

                if (!jumpSfxPlayed)
                {
                    jumpSfxPlayed = true;
                    ServiceProvider.GetService<AudioManager>().PlaySFX(jumpSfxIndex);
                }

                Debug.Log("Detection");
            }
            else if (!isInside)
            {
                runnersInside.Remove(runner);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, detectionBoxSize);
    }
}
