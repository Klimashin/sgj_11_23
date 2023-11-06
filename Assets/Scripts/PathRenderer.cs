using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private LineRenderer LineRenderer => _lineRenderer ??= GetComponent<LineRenderer>();

    public int LastPointIndex => _lineRenderer.positionCount - 1;

    public void AddPathSegment(Vector3[] pathSegmentData)
    {
        Vector3 currentPathLastPoint = LineRenderer.GetPosition(LineRenderer.positionCount - 1);
        Vector3[] cutSegmentData = new Vector3[pathSegmentData.Length - 1]; 
        for (var i = 1; i < pathSegmentData.Length; i++)
        {
            cutSegmentData[i - 1] = pathSegmentData[i] + currentPathLastPoint;
        }
            
        Vector3[] currentPath = new Vector3[LineRenderer.positionCount];
        LineRenderer.GetPositions(currentPath);
        Vector3[] combined = currentPath.Concat(cutSegmentData).ToArray();
        LineRenderer.positionCount = combined.Length;
        LineRenderer.SetPositions(combined);
    }

    public Vector3 GetPointPosition(int positionIndex)
    {
        return LineRenderer.GetPosition(positionIndex);
    }
}
