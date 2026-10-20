#if UNITY_EDITOR
using UnityEditor;
#endif
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
    [SerializeField] private RectTransform rouletteImage;
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (rouletteImage == null || bonuses == null || bonuses.Length == 0)
        {
            return;
        }

        float totalSize = 0f;

        for (int i = 0; i < bonuses.Length; i++)
        {
            totalSize += bonuses[i].Size;
        }

        if (totalSize <= 0f)
        {
            return;
        }

        Vector3[] corners = new Vector3[4];
        rouletteImage.GetWorldCorners(corners);

        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);
        float radius = Mathf.Min(width, height) * 0.5f;
        Vector3 center = (corners[0] + corners[2]) * 0.5f;
        Vector3 normal = rouletteImage.forward;
        Vector3 zeroDirection = rouletteImage.up;

        float firstSectionAngle = bonuses[0].Size / totalSize * 360f;
        float sectionStartAngle = -firstSectionAngle * 0.5f;

        for (int i = 0; i < bonuses.Length; i++)
        {
            float sectionAngle = bonuses[i].Size / totalSize * 360f;
            Vector3 sectionStartDirection = Quaternion.AngleAxis(
                -sectionStartAngle,
                normal) * zeroDirection;

            Color sectionColor = Color.HSVToRGB(i / (float)bonuses.Length, 0.75f, 1f);
            Color sectionFillColor = sectionColor;
            sectionFillColor.a = 0.2f;

            Handles.color = sectionFillColor;
            Handles.DrawSolidArc(
                center,
                -normal,
                sectionStartDirection,
                sectionAngle,
                radius);

            Handles.color = sectionColor;
            Handles.DrawWireArc(
                center,
                -normal,
                sectionStartDirection,
                sectionAngle,
                radius);
            Handles.DrawLine(center, center + sectionStartDirection * radius);

            float sectionCenterAngle = sectionStartAngle + sectionAngle * 0.5f;
            Vector3 labelDirection = Quaternion.AngleAxis(
                -sectionCenterAngle,
                normal) * zeroDirection;
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = sectionColor;

            Handles.Label(
                center + labelDirection * radius * 0.65f,
                $"{i}: {bonuses[i].RunnerAmount:+0;-0;0}\n{sectionAngle:0.#} deg",
                labelStyle);

            sectionStartAngle += sectionAngle;
        }
    }
#endif
}
