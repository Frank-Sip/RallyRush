using UnityEngine;
using System.Collections.Generic;

public class RunnerAttackerComponent : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector3 runnerAttackBoxSize = new Vector3(4f, 2f, 4f);

    private bool isAttacking = true;
    private Enemy enemy;
    private CrowdSystem crowdSystem;

    public bool IsAttacking
    {
        get => isAttacking;
        set => isAttacking = value;
    }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        if (isAttacking)
        {
            DetectRunners();
        }
    }

    public void DetectRunners()
    {
        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        Vector3 halfBoxSize = runnerAttackBoxSize * 0.5f;

        for (int i = runners.Count - 1; i >= 0; i--)
        {
            Vector3 runnerPosition = attackPoint.InverseTransformPoint(runners[i].transform.position);

            if (Mathf.Abs(runnerPosition.x) <= halfBoxSize.x && Mathf.Abs(runnerPosition.y) <= halfBoxSize.y && Mathf.Abs(runnerPosition.z) <= halfBoxSize.z)
            {
                runners[i].Die();
            }
        }
    }
    
     private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, runnerAttackBoxSize);
        Gizmos.matrix = previousMatrix;
    }
}
