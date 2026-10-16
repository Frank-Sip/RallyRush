using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [Min(2)][SerializeField] private int floorAmount = 2;
    [SerializeField] private FloorContainer floorContainer;

    private void Awake()
    {
        FloorData[] selectedFloors = SelectFloors();
        SpawnFloors(selectedFloors);
    }

    private FloorData[] SelectFloors()
    {
        FloorData[] selectedFloors = new FloorData[floorAmount];
        HashSet<GameObject> usedFloors = new HashSet<GameObject>();

        selectedFloors[0] = GetRandomUnusedFloor(floorContainer.FirstFloors, usedFloors, "first");

        selectedFloors[floorAmount - 1] = GetRandomUnusedFloor(floorContainer.LastFloors, usedFloors, "last");

        for (int i = 1; i < floorAmount - 1; i++)
        {
            selectedFloors[i] = GetRandomUnusedFloor(floorContainer.MiddleFloors, usedFloors, "middle");
        }

        return selectedFloors;
    }

    private static FloorData GetRandomUnusedFloor(
        FloorData[] floorOptions,
        HashSet<GameObject> usedFloors,
        string floorGroup)
    {
        List<FloorData> availableFloors = new List<FloorData>();

        for (int i = 0; i < floorOptions.Length; i++)
        {
            if (!usedFloors.Contains(floorOptions[i].Prefab))
            {
                availableFloors.Add(floorOptions[i]);
            }
        }

        FloorData selectedFloor = availableFloors[Random.Range(0, availableFloors.Count)];
        usedFloors.Add(selectedFloor.Prefab);
        return selectedFloor;
    }

    private void SpawnFloors(FloorData[] selectedFloors)
    {
        Vector3 floorPosition = Vector3.zero;

        for (int i = 0; i < selectedFloors.Length; i++)
        {
            FloorData floorData = selectedFloors[i];

            if (i > 0)
            {
                floorPosition.z += floorData.Length / 2;
            }

            GameObject floorInstance = Instantiate(floorData.Prefab, transform);
            floorInstance.transform.localPosition = floorPosition;
            floorInstance.transform.localRotation = Quaternion.identity;

            floorPosition.z += floorData.Length / 2;
        }
    }
}
