using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyGroup : EnemyGroup
{
    [SerializeField] private float shootPointHeight = 1f;

    private string ShootAnimationTrigger = "Shoot";
    private RangedAttack rangedAttack;

    protected override void Awake()
    {
        base.Awake();
        rangedAttack = GetComponent<RangedAttack>();
        rangedAttack.OnStartShooting += SetShootAnimation;
    }

    protected override void Start()
    {
        base.Start();
        EnemiesAmountChanged += UpdateShootPoints;
        UpdateShootPoints(GetEnemiesAmount());
    }

    private void UpdateShootPoints(int enemiesAmount)
    {
        Enemy[] enemies = GetComponentsInChildren<Enemy>(false);
        List<Transform> shootPoints = new List<Transform>(enemiesAmount);

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            shootPoints.Add(enemy.transform);
        }

        Vector3 shootPointOffset = Vector3.up * shootPointHeight;
        rangedAttack.SetShootPoints(shootPoints.ToArray(), shootPointOffset);
    }

    protected override void DetectCrowd()
    {
        base.DetectCrowd();
        rangedAttack.SetCanShoot(false);
    }

    public void SetShootAnimation()
    {
        Enemy[] enemies = GetComponentsInChildren<Enemy>(false);

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].SetAnimTrigger(ShootAnimationTrigger);
        }
    }
}
