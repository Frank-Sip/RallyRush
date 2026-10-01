using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Image healthFillImage;

    private void Start()
    {
        healthComponent.OnDamageTaken += UpdateHealthUI;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthFillImage.fillAmount = healthComponent.CurrentHealth / healthComponent.MaxHealth;
    }

    private void OnDestroy()
    {
        healthComponent.OnDamageTaken -= UpdateHealthUI;
    }
}
