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
        FloorData[] firstFloors = (FloorData[])floorContainer.FirstFloors.Clone();
        FloorData[] middleFloors = (FloorData[])floorContainer.MiddleFloors.Clone();
        FloorData[] lastFloors = (FloorData[])floorContainer.LastFloors.Clone();

        int firstFloorCount = firstFloors.Length;
        int middleFloorCount = middleFloors.Length;
        int lastFloorCount = lastFloors.Length;

        selectedFloors[0] = GetRandomUnusedFloor(firstFloors, ref firstFloorCount, "first");

        selectedFloors[floorAmount - 1] = GetRandomUnusedFloor(lastFloors, ref lastFloorCount, "last");

        for (int i = 1; i < floorAmount - 1; i++)
        {
            selectedFloors[i] = GetRandomUnusedFloor(middleFloors, ref middleFloorCount, "middle");
        }

        return selectedFloors;
    }

    private static FloorData GetRandomUnusedFloor(FloorData[] availableFloors, ref int availableFloorCount, string floorGroup)
    {
        int selectedIndex = Random.Range(0, availableFloorCount);
        FloorData selectedFloor = availableFloors[selectedIndex];

        availableFloorCount--;
        availableFloors[selectedIndex] = availableFloors[availableFloorCount];

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
