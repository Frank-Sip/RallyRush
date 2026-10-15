using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [Min(2)][SerializeField] private int floorAmount = 2;
    [SerializeField] private FloorContainer floorContainer;

    private void Awake()
    {
        Floor[] selectedFloors = SelectFloors();
        SpawnFloors(selectedFloors);
    }

    private Floor[] SelectFloors()
    {
        Floor[] selectedFloors = new Floor[floorAmount];
        HashSet<Floor> usedFloors = new HashSet<Floor>();

        selectedFloors[0] = GetRandomUnusedFloor(
            floorContainer.FirstFloors,
            usedFloors,
            "first");

        selectedFloors[floorAmount - 1] = GetRandomUnusedFloor(
            floorContainer.LastFloors,
            usedFloors,
            "last");

        for (int i = 1; i < floorAmount - 1; i++)
        {
            selectedFloors[i] = GetRandomUnusedFloor(
                floorContainer.MiddleFloors,
                usedFloors,
                "middle");
        }

        return selectedFloors;
    }

    private static Floor GetRandomUnusedFloor(
        Floor[] floorOptions,
        HashSet<Floor> usedFloors,
        string floorGroup)
    {
        List<Floor> availableFloors = new List<Floor>();

        for (int i = 0; i < floorOptions.Length; i++)
        {
            if (!usedFloors.Contains(floorOptions[i]))
            {
                availableFloors.Add(floorOptions[i]);
            }
        }

        if (availableFloors.Count == 0)
        {
            throw new System.InvalidOperationException(
                $"FloorContainer does not have enough unique {floorGroup} floors.");
        }

        Floor selectedFloor = availableFloors[Random.Range(0, availableFloors.Count)];
        usedFloors.Add(selectedFloor);
        return selectedFloor;
    }

    private void SpawnFloors(Floor[] selectedFloors)
    {
        Vector3 floorPosition = Vector3.zero;

        for (int i = 0; i < selectedFloors.Length; i++)
        {
            Floor floorPrefab = selectedFloors[i];

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
