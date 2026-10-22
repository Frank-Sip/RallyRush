using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class RangedEnemyGroup : EnemyGroup
{
    [SerializeField] private float shootPointHeight = 1f;

    private string ShootAnimationTrigger = "Shoot";
    private RangedAttack rangedAttack;

    public override void CustomAwake()
    {
        base.CustomAwake();
        rangedAttack = GetComponent<RangedAttack>();
        rangedAttack.CustomAwake();
        rangedAttack.OnStartShooting += SetShootAnimation;
    }

    public override void CustomStart()
    {
        base.CustomStart();
        EnemiesAmountChanged += UpdateShootPoints;
        UpdateShootPoints(GetEnemiesAmount());
    }

    public override void CustomUpdate()
    {
        base.CustomUpdate();
        rangedAttack.CustomUpdate();
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
