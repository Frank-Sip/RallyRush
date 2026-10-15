using UnityEngine;

[System.Serializable]
public class DoorValueRange
{
    [Min(1)] [SerializeField] private int minimumValue = 2;
    [Min(1)] [SerializeField] private int maximumValue = 30;

    public int GetRandomValue()
    {
        int maximum = Mathf.Max(minimumValue, maximumValue);
        return Random.Range(minimumValue, maximum + 1);
    }
}

[CreateAssetMenu(fileName = "DoorSettings", menuName = "Scriptable Objects/Door Settings")]
public class DoorSettingsSO : ScriptableObject
{
    [Header("Value Ranges")]
    [SerializeField] private DoorValueRange additionValueRange = new DoorValueRange();
    [SerializeField] private DoorValueRange subtractionValueRange = new DoorValueRange();
    [SerializeField] private DoorValueRange multiplicationValueRange = new DoorValueRange();
    [SerializeField] private DoorValueRange divisionValueRange = new DoorValueRange();

    [Header("Door Colors")]
    [SerializeField] private Color bonusColor;
    [SerializeField] private Color penaltyColor;

    public Color BonusColor => bonusColor;
    public Color PenaltyColor => penaltyColor;

    public int GetRandomValue(BonusType bonusType)
    {
        switch (bonusType)
        {
            case BonusType.Addition:
                return additionValueRange.GetRandomValue();
            case BonusType.Subtraction:
                return subtractionValueRange.GetRandomValue();
            case BonusType.Multiplication:
                return multiplicationValueRange.GetRandomValue();
            case BonusType.Division:
                return divisionValueRange.GetRandomValue();
            default:
                throw new System.ArgumentOutOfRangeException(nameof(bonusType), bonusType, null);
        }
    }
}
