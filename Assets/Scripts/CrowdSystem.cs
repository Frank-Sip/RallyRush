using UnityEngine;

public class CrowdSystem : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private float angle = 1f;

    private void Update()
    {
        PlaceRunner();
    }

    private void PlaceRunner()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 childPosition = CalculateRunnerPosition(i);
            transform.GetChild(i).localPosition = childPosition;
        }
    }
    
    private Vector3 CalculateRunnerPosition(int index)
    {
        float x = radius * Mathf.Sqrt(index) * Mathf.Cos(Mathf.Deg2Rad * index * angle);
        float z = radius * Mathf.Sqrt(index) * Mathf.Sin(Mathf.Deg2Rad * index * angle);
        return new Vector3(x, 0f, z);
    }
}
