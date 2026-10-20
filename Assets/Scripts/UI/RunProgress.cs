using UnityEngine;
using UnityEngine.UI;

public class RunProgress : MonoBehaviour
{
    [SerializeField] private Image progressFill;
    [SerializeField] private string finishTag;

    private Transform playerTransform;
    private Vector3 startPosition;
    private Vector3 levelDirection;
    private float levelDistanceSqr;
    private bool initialized;

    private void Awake()
    {
        ServiceProvider.RegisterService(this);
    }

    public void CustomStart()
    {
        if (initialized)
        {
            return;
        }

        playerTransform = ServiceProvider.GetService<CrowdSystem>().transform;
        Transform finishTransform = GameObject.FindGameObjectWithTag(finishTag).transform;

        startPosition = playerTransform.position;
        levelDirection = finishTransform.position - startPosition;
        levelDirection.y = 0f;
        levelDistanceSqr = levelDirection.sqrMagnitude;
        progressFill.fillAmount = 0f;
        initialized = true;
    }

    public void CustomUpdate()
    {
        Vector3 playerDistance = playerTransform.position - startPosition;
        playerDistance.y = 0f;

        float progress = Vector3.Dot(playerDistance, levelDirection) / levelDistanceSqr;
        progressFill.fillAmount = Mathf.Clamp01(progress);
    }
}
