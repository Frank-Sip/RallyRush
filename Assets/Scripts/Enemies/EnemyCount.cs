using UnityEngine;
using TMPro;

public class EnemyCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI enemyCountText;

    private EnemyGroup enemyGroup;

    private void Start()
    {
        enemyGroup = GetComponentInParent<EnemyGroup>();
        enemyGroup.EnemiesAmountChanged += UpdateEnemyCount;
        UpdateEnemyCount(enemyGroup.GetEnemiesAmount());
    }

    private void OnDestroy()
    {
        enemyGroup.EnemiesAmountChanged -= UpdateEnemyCount;
    }

    private void UpdateEnemyCount(int amount)
    {
        enemyCountText.text = amount.ToString();
    }
}
