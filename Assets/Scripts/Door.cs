using UnityEngine;
using TMPro;

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

    [Header("Door Colors")]
    [SerializeField] private Color bonusColor;
    [SerializeField] private Color penaltyColor;

    private void Awake()
    {
        SetDoors();
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
}