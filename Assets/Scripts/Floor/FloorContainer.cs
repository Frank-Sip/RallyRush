using UnityEngine;

[CreateAssetMenu(fileName = "FloorContainer", menuName = "Scriptable Objects/FloorContainer")]
public class FloorContainer : ScriptableObject
{
    [Header("First Floors")]
    [SerializeField] private Floor[] firstFloors;

    [Header("Middle Floors")]
    [SerializeField] private Floor[] middleFloors;

    [Header("Last Floors")]
    [SerializeField] private Floor[] lastFloors;

    public Floor[] FirstFloors => firstFloors;
    public Floor[] MiddleFloors => middleFloors;
    public Floor[] LastFloors => lastFloors;
}
