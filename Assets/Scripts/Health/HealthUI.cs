using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Image healthFillImage;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        healthComponent.OnDamageTaken += UpdateHealthUI;
        UpdateHealthUI();
    }

    private void Update()
    {
        healthFillImage.transform.LookAt(healthFillImage.transform.position + cameraTransform.forward, cameraTransform.up);
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
