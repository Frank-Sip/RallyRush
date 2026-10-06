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

    [Header("Settings")]
    [SerializeField] private DoorSettingsSO doorSettings;

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

    private void RandomizeDoors()
    {
        int guaranteedBonusIndex = Random.Range(0, doorOptions.Length);

        for (int i = 0; i < doorOptions.Length; i++)
        {
            BonusType bonusType = i == guaranteedBonusIndex
                ? GetRandomPositiveBonus()
                : GetRandomBonus();

            doorOptions[i].SetBonus(bonusType, doorSettings.GetRandomValue(bonusType));
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

    private void SetDoors()
    {
        for (int i = 0; i < doorOptions.Length; i++)
        {
            DoorOption option = doorOptions[i];

            switch (option.BonusType)
            {
                case BonusType.Addition:
                    option.SetVisual("+" + option.Value, doorSettings.BonusColor);
                    break;
                case BonusType.Subtraction:
                    option.SetVisual("-" + option.Value, doorSettings.PenaltyColor);
                    break;
                case BonusType.Multiplication:
                    option.SetVisual("x" + option.Value, doorSettings.BonusColor);
                    break;
                case BonusType.Division:
                    option.SetVisual("/" + option.Value, doorSettings.PenaltyColor);
                    break;
            }
        }
    }

    private void DetectRunnerCollision()
    {
        if (wasUsed) return;

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
