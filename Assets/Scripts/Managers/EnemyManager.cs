using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class EnemyManager
{
    private readonly List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
    private readonly List<BigEnemyReposition> bigEnemies = new List<BigEnemyReposition>();

    public void CustomAwake()
    {
        ServiceProvider.RegisterService(this);
        enemyGroups.AddRange(Object.FindObjectsByType<EnemyGroup>(FindObjectsInactive.Include));
        bigEnemies.AddRange(Object.FindObjectsByType<BigEnemyReposition>(FindObjectsInactive.Include));

        for (int i = 0; i < enemyGroups.Count; i++)
        {
            enemyGroups[i].CustomAwake();
        }

        for (int i = 0; i < bigEnemies.Count; i++)
        {
            bigEnemies[i].CustomAwake();
        }
    }

    public void CustomStart()
    {
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            enemyGroups[i].CustomStart();
        }

        for (int i = 0; i < bigEnemies.Count; i++)
        {
            bigEnemies[i].CustomStart();
        }
    }

    public void CustomUpdate()
    {
        UpdateEnemyGroups();
        UpdateBigEnemies();
    }

    private void UpdateEnemyGroups()
    {
        for (int i = enemyGroups.Count - 1; i >= 0; i--)
        {
            EnemyGroup enemyGroup = enemyGroups[i];

            if (enemyGroup == null)
            {
                enemyGroups.RemoveAt(i);
                continue;
            }

            if (enemyGroup.isActiveAndEnabled)
            {
                enemyGroup.CustomUpdate();
            }
        }
    }

    private void UpdateBigEnemies()
    {
        for (int i = bigEnemies.Count - 1; i >= 0; i--)
        {
            BigEnemyReposition bigEnemy = bigEnemies[i];

            if (bigEnemy == null)
            {
                bigEnemies.RemoveAt(i);
                continue;
            }

            if (bigEnemy.isActiveAndEnabled)
            {
                bigEnemy.CustomUpdate();
            }
        }
    }
}
