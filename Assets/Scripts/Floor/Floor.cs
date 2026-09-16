using UnityEngine;

public class Floor : MonoBehaviour
{
    private MeshFilter GetFloorMeshFilter()
    {
        return GetComponentInChildren<MeshFilter>();
    }

    public float GetLength()
    {
        MeshFilter meshFilter = GetFloorMeshFilter();
        return meshFilter.sharedMesh.bounds.size.z * Mathf.Abs(meshFilter.transform.lossyScale.z);
    }
}