using System.Collections.Generic;
using UnityEngine;
using TMPro;

[SelectionBase]
public class Door : MonoBehaviour
{
    enum BonusType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    [Header("Visual Elements")]
    [SerializeField] private SpriteRenderer rightDoorRenderer;
    [SerializeField] private SpriteRenderer leftDoorRenderer;
    [SerializeField] private TextMeshProUGUI rightDoorText;
    [SerializeField] private TextMeshProUGUI leftDoorText;

    [Header("Right Door Settings")]
    [SerializeField] private BonusType rightDoorBonus;
    [SerializeField] private int rightDoorValue;

    [Header("Left Door Settings")]
    [SerializeField] private BonusType leftDoorBonus;
    [SerializeField] private int leftDoorValue;

    [Header("Random Value Settings")]
    [Min(1)]
    [SerializeField] private int minimumValue = 2;
    [Min(1)]
    [SerializeField] private int maximumValue = 30;

    [Header("Door Colors")]
    [SerializeField] private Color bonusColor;
    [SerializeField] private Color penaltyColor;

    [Header("Collision Detection")]
    [Min(0f)]
    [SerializeField] private float collisionDistance = 0.15f;

    private CrowdSystem crowdSystem;
    private bool wasUsed;

    private void Awake()
    {
        RandomizeDoors();
        SetDoors();
    }

    private void Start()
    {
        crowdSystem = ServiceProvider.GetService<CrowdSystem>();
    }

    private void Update()
    {
        DetectRunnerCollision();
    }

    private void OnValidate()
    {
        minimumValue = Mathf.Max(1, minimumValue);
        maximumValue = Mathf.Max(minimumValue, maximumValue);
    }

    private void RandomizeDoors()
    {
        bool guaranteeBonusOnRightDoor = Random.value < 0.5f;
        BonusType guaranteedBonus = GetRandomPositiveBonus();
        BonusType randomBonus = GetRandomBonus();

        if (guaranteeBonusOnRightDoor)
        {
            rightDoorBonus = guaranteedBonus;
            leftDoorBonus = randomBonus;
        }
        else
        {
            leftDoorBonus = guaranteedBonus;
            rightDoorBonus = randomBonus;
        }

        rightDoorValue = GetRandomValue();
        leftDoorValue = GetRandomValue();
    }

    private BonusType GetRandomPositiveBonus()
    {
        return Random.value < 0.5f ? BonusType.Addition : BonusType.Multiplication;
    }

    private BonusType GetRandomBonus()
    {
        return (BonusType)Random.Range(0, (int)BonusType.Division + 1);
    }

    private int GetRandomValue()
    {
        int min = Mathf.Max(1, minimumValue);
        int max = Mathf.Max(min, maximumValue);
        return Random.Range(min, max + 1);
    }

    private void SetDoors()
    {
        switch (rightDoorBonus)
        {
            case BonusType.Addition:
                rightDoorText.text = "+" + rightDoorValue;
                rightDoorRenderer.color = bonusColor;
                break;
            case BonusType.Subtraction:
                rightDoorText.text = "-" + rightDoorValue;
                rightDoorRenderer.color = penaltyColor;
                break;
            case BonusType.Multiplication:
                rightDoorText.text = "x" + rightDoorValue;
                rightDoorRenderer.color = bonusColor;
                break;
            case BonusType.Division:
                rightDoorText.text = "/" + rightDoorValue;
                rightDoorRenderer.color = penaltyColor;
                break;
        }

        switch (leftDoorBonus)
        {
            case BonusType.Addition:
                leftDoorText.text = "+" + leftDoorValue;
                leftDoorRenderer.color = bonusColor;
                break;
            case BonusType.Subtraction:
                leftDoorText.text = "-" + leftDoorValue;
                leftDoorRenderer.color = penaltyColor;
                break;
            case BonusType.Multiplication:
                leftDoorText.text = "x" + leftDoorValue;
                leftDoorRenderer.color = bonusColor;
                break;
            case BonusType.Division:
                leftDoorText.text = "/" + leftDoorValue;
                leftDoorRenderer.color = penaltyColor;
                break;
        }
    }

    private void DetectRunnerCollision()
    {
        if (wasUsed) return;


        Vector3 doorCenter = (leftDoorRenderer.bounds.center + rightDoorRenderer.bounds.center) / 2f;
        float distanceToDoorPlane = Mathf.Abs(Vector3.Dot(crowdSystem.transform.position - doorCenter, transform.forward));

        if (distanceToDoorPlane > crowdSystem.GetCrowdRadius() + collisionDistance) return;

        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        float collisionDistanceSqr = collisionDistance * collisionDistance;
        float closestDistanceSqr = collisionDistanceSqr;
        BonusType selectedBonus = default;
        int selectedValue = 0;
        bool collisionDetected = false;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 runnerPosition = runners[i].transform.position;
            float rightDistanceSqr = GetSqrDistanceXZ(runnerPosition, rightDoorRenderer.bounds);
            float leftDistanceSqr = GetSqrDistanceXZ(runnerPosition, leftDoorRenderer.bounds);

            if (rightDistanceSqr <= closestDistanceSqr)
            {
                closestDistanceSqr = rightDistanceSqr;
                selectedBonus = rightDoorBonus;
                selectedValue = rightDoorValue;
                collisionDetected = true;
            }

            if (leftDistanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = leftDistanceSqr;
                selectedBonus = leftDoorBonus;
                selectedValue = leftDoorValue;
                collisionDetected = true;
            }
        }

        if (!collisionDetected) return;

        SetRunnerAmount(selectedBonus, selectedValue);
        wasUsed = true;
    }

    private void SetRunnerAmount(BonusType bonusType, int configuredValue)
    {
        int amount = crowdSystem.GetRunnersAmount();
        int value = Mathf.Max(1, configuredValue);

        switch (bonusType)
        {
            case BonusType.Addition:
                amount += value;
                break;
            case BonusType.Subtraction:
                amount -= value;
                break;
            case BonusType.Multiplication:
                amount *= value;
                break;
            case BonusType.Division:
                amount /= value;
                break;
        }

        crowdSystem.SetRunnersAmount(amount);
    }

    private static float GetSqrDistanceXZ(Vector3 point, Bounds bounds)
    {
        float closestX = Mathf.Clamp(point.x, bounds.min.x, bounds.max.x);
        float closestZ = Mathf.Clamp(point.z, bounds.min.z, bounds.max.z);
        float xDistance = point.x - closestX;
        float zDistance = point.z - closestZ;

        return xDistance * xDistance + zDistance * zDistance;
    }
}