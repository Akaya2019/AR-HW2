using UnityEngine;

public class PointCloudVisualizer : MonoBehaviour
{
    public GameObject pointPrefab; // Inspector'dan atayýn
    public Transform pointCloudParent; // Noktalarýn parent'ý olacak transform


    public void VisualizePoints(Vector3[] points, Color color, Transform parent)
    {
        foreach (Vector3 point in points)
        {
            GameObject pointInstance = Instantiate(pointPrefab, point, Quaternion.identity, parent);
            pointInstance.GetComponent<Renderer>().material.color = color;
        }
    }
}
