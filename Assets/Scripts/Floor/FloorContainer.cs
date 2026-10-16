using UnityEngine;

[System.Serializable]
public class FloorData
{
    [SerializeField] private GameObject prefab;
    [Min(0f)][SerializeField] private float length;

    public GameObject Prefab => prefab;
    public float Length => length;
}

[CreateAssetMenu(fileName = "FloorContainer", menuName = "Scriptable Objects/FloorContainer")]
public class FloorContainer : ScriptableObject
{
    [Header("First Floors")]
    [SerializeField] private FloorData[] firstFloors;

    [Header("Middle Floors")]
    [SerializeField] private FloorData[] middleFloors;

    [Header("Last Floors")]
    [SerializeField] private FloorData[] lastFloors;

    public FloorData[] FirstFloors => firstFloors;
    public FloorData[] MiddleFloors => middleFloors;
    public FloorData[] LastFloors => lastFloors;
}
