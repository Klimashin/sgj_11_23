using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathRenderer : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private LineRenderer plannedPathRenderer;
    [SerializeField] private LineRenderer completedPathRenderer;
    [SerializeField] private LineRenderer previewPathRenderer;
    [SerializeField] private Marker marker;
    [SerializeField] private float completedPathUpdateCooldown = 0.1f;

    private readonly List<Vector3> _positions = new ();
    private float _currentUpdateCooldown = 0f;

    public int LastPlannedPointIndex => plannedPathRenderer.positionCount - 1;

    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<CardDragStarted>(this, OnCardDragStarted);
        GameController.Instance.eventsDispatcher.Register<CardDragEnded>(this, OnCardDragEnded);
    }

    public void AddPathSegment(Vector3[] pathSegmentData)
    {
        Vector3 currentPathLastPoint = plannedPathRenderer.GetPosition(plannedPathRenderer.positionCount - 1);
        Vector3[] cutSegmentData = new Vector3[pathSegmentData.Length - 1]; 
        for (var i = 1; i < pathSegmentData.Length; i++)
        {
            cutSegmentData[i - 1] = pathSegmentData[i] + currentPathLastPoint;
        }
            
        Vector3[] currentPath = new Vector3[plannedPathRenderer.positionCount];
        plannedPathRenderer.GetPositions(currentPath);
        Vector3[] combined = currentPath.Concat(cutSegmentData).ToArray();
        plannedPathRenderer.positionCount = combined.Length;
        plannedPathRenderer.SetPositions(combined);
    }

    public Vector3 GetPointPosition(int positionIndex)
    {
        return plannedPathRenderer.GetPosition(positionIndex);
    }
    
    private void Update()
    {
        if (Time.timeScale <= float.Epsilon)
        {
            return;
        }

        _currentUpdateCooldown -= Time.deltaTime;
        if (_currentUpdateCooldown <= 0f)
        {
            _positions.Add(marker.transform.position);
            completedPathRenderer.positionCount = _positions.Count;
            completedPathRenderer.SetPositions(_positions.ToArray());
            _currentUpdateCooldown = completedPathUpdateCooldown;
        }
    }
    
    private void OnCardDragEnded(CardDragEnded cardDragEnded)
    {
        previewPathRenderer.enabled = false;
    }

    private void OnCardDragStarted(CardDragStarted cardDragStarted)
    {
        var currentPathLastPointIndex = GameController.Instance.PathRenderer.LastPlannedPointIndex;
        var currentPathLastPoint = GameController.Instance.PathRenderer.GetPointPosition(currentPathLastPointIndex);
        var segmentToPreview = cardDragStarted.segmentLineRenderer;

        Vector3[] positions = new Vector3[segmentToPreview.positionCount];
        segmentToPreview.GetPositions(positions);
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] += currentPathLastPoint;
        }

        previewPathRenderer.positionCount = positions.Length;
        previewPathRenderer.SetPositions(positions);
        previewPathRenderer.enabled = true;
    }

    private void OnDestroy()
    {
        GameController.Instance.eventsDispatcher.Unregister<CardDragStarted>(this);
        GameController.Instance.eventsDispatcher.Unregister<CardDragEnded>(this);
    }
}
