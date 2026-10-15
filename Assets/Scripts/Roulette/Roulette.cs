using UnityEngine;

[System.Serializable]
public class RouletteBonus
{
    [SerializeField] private int runnerAmount;
    [Min(0f)][SerializeField] private float size = 1f;

    public int RunnerAmount => runnerAmount;
    public float Size => size;
}

public class Roulette : MonoBehaviour
{
    [Header("Roulette Settings")]
    [SerializeField] private GameObject arrow;
    [Min(0f)][SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private RouletteBonus[] bonuses;

    private bool isSpinning;

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
        gameObject.SetActive(false);
    }

    public void StartRoulette()
    {
        gameObject.SetActive(true);
        isSpinning = true;
    }

    public void CustomUpdate()
    {
        if (!isSpinning)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StopRoulette();
            return;
        }

        arrow.transform.Rotate(0f, 0f, -rotationSpeed * Time.unscaledDeltaTime);
    }

    private void StopRoulette()
    {
        isSpinning = false;
        int runnerBonus = GetCurrentBonus();
        CrowdSystem crowdSystem = ServiceProvider.GetService<CrowdSystem>();

        crowdSystem.SetRunnersAmount(crowdSystem.GetRunnersAmount() + runnerBonus);
        gameObject.SetActive(false);
        GameManager.Instance.SetGameStatus(GameStatus.Gameplay);
    }

    private int GetCurrentBonus()
    {
        float totalSize = 0f;

        for (int i = 0; i < bonuses.Length; i++)
        {
            totalSize += bonuses[i].Size;
        }

        float firstBonusAngleSize = bonuses[0].Size / totalSize * 360f;
        float firstBonusStartAngle = -firstBonusAngleSize * 0.5f;
        float arrowAngle = Mathf.Repeat(-arrow.transform.localEulerAngles.z - firstBonusStartAngle, 360f);
        float accumulatedAngle = 0f;

        for (int i = 0; i < bonuses.Length; i++)
        {
            accumulatedAngle += bonuses[i].Size / totalSize * 360f;

            if (arrowAngle < accumulatedAngle)
            {
                return bonuses[i].RunnerAmount;
            }
        }

        return bonuses[bonuses.Length - 1].RunnerAmount;
    }
}
