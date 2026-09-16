using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [SerializeField] private int floorAmount;
    [SerializeField] private Floor[] floors;
    
    private void Awake()
    {
        Vector3 floorPosition = Vector3.zero;
        for (int i = 0; i < floorAmount; i++)
        {
            Floor floorPrefab = floors[Random.Range(0, floors.Length)];

            if (i > 0)
            {
                floorPosition.z += floorPrefab.GetLength() / 2;
            }

            Floor floorInstance = Instantiate(floorPrefab, transform);
            floorInstance.transform.localPosition = floorPosition;
            floorInstance.transform.localRotation = Quaternion.identity;

            floorPosition.z += floorInstance.GetLength() / 2;
        }
    }
}
