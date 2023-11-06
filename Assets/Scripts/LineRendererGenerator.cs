using PathCreation;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(PathCreator))]
public class LineRendererGenerator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private PathCreator pathCreator;

    public LineRenderer LineRenderer => lineRenderer;
    public PathCreator PathCreator => pathCreator;
}
