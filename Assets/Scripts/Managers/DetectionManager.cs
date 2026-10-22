using UnityEngine;

public class DetectionManager
{
    private DetectCrowdComponent[] detectors;

    public void CustomAwake()
    {
        ServiceProvider.RegisterService(this);
    }

    public void CustomStart()
    {
        detectors = Object.FindObjectsByType<DetectCrowdComponent>(FindObjectsInactive.Include);
    }

    public void CustomUpdate()
    {
        for (int i = 0; i < detectors.Length; i++)
        {
            if (detectors[i].isActiveAndEnabled)
            {
                detectors[i].CustomUpdate();
            }
        }
    }
}
