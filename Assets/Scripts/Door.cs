using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DoorOption
{
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private BonusType bonusType;
    [SerializeField] private int value;

    public Bounds Bounds => renderer.bounds;
    public BonusType BonusType => bonusType;
    public int Value => value;
    public bool IsValid => renderer != null && text != null;

    public void SetBonus(BonusType type, int newValue)
    {
        bonusType = type;
        value = newValue;
    }

    public void SetVisual(string label, Color color)
    {
        text.text = label;
        renderer.color = color;
    }
}

public enum BonusType
{
    Addition,
    Subtraction,
    Multiplication,
    Division
}

[SelectionBase]
public class Door : MonoBehaviour
{
    [Header("Door Options")]
    [SerializeField] private DoorOption[] doorOptions;

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
        int guaranteedBonusIndex = Random.Range(0, doorOptions.Length);

        for (int i = 0; i < doorOptions.Length; i++)
        {
            BonusType bonusType = i == guaranteedBonusIndex
                ? GetRandomPositiveBonus()
                : GetRandomBonus();

            doorOptions[i].SetBonus(bonusType, GetRandomValue());
        }
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
        for (int i = 0; i < doorOptions.Length; i++)
        {
            DoorOption option = doorOptions[i];

            switch (option.BonusType)
            {
                case BonusType.Addition:
                    option.SetVisual("+" + option.Value, bonusColor);
                    break;
                case BonusType.Subtraction:
                    option.SetVisual("-" + option.Value, penaltyColor);
                    break;
                case BonusType.Multiplication:
                    option.SetVisual("x" + option.Value, bonusColor);
                    break;
                case BonusType.Division:
                    option.SetVisual("/" + option.Value, penaltyColor);
                    break;
            }
        }
    }

    private void DetectRunnerCollision()
    {
        if (wasUsed) return;


        Vector3 doorCenter = GetDoorCenter();
        float distanceToDoorPlane = Mathf.Abs(Vector3.Dot(crowdSystem.transform.position - doorCenter, transform.forward));

        if (distanceToDoorPlane > crowdSystem.GetCrowdRadius() + collisionDistance) return;

        IReadOnlyList<Runner> runners = crowdSystem.GetRunners();
        float collisionDistanceSqr = collisionDistance * collisionDistance;
        float closestDistanceSqr = collisionDistanceSqr;
        DoorOption selectedOption = null;

        for (int i = 0; i < runners.Count; i++)
        {
            Vector3 runnerPosition = runners[i].transform.position;

            for (int optionIndex = 0; optionIndex < doorOptions.Length; optionIndex++)
            {
                DoorOption option = doorOptions[optionIndex];
                float distanceSqr = GetSqrDistanceXZ(runnerPosition, option.Bounds);

                if (distanceSqr <= closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    selectedOption = option;
                }
            }
        }

        if (selectedOption == null) return;

        SetRunnerAmount(selectedOption.BonusType, selectedOption.Value);
        wasUsed = true;
    }

    private Vector3 GetDoorCenter()
    {
        Vector3 center = Vector3.zero;

        for (int i = 0; i < doorOptions.Length; i++)
        {
            center += doorOptions[i].Bounds.center;
        }

        return center / doorOptions.Length;
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
