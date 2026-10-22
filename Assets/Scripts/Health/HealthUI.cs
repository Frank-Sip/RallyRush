using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Image healthFillImage;

    private Transform cameraTransform;

    public void CustomAwake()
    {
        cameraTransform = Camera.main.transform;
        healthComponent.OnDamageTaken += UpdateHealthUI;
        UpdateHealthUI();
    }

    public void CustomUpdate()
    {
        transform.LookAt(transform.position + cameraTransform.forward, cameraTransform.up);
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
