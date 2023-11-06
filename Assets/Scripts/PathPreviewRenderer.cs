using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathPreviewRenderer : MonoBehaviour, IEventsDispatcherClient
{
    
    private LineRenderer _lineRenderer;
    private LineRenderer LineRenderer => _lineRenderer ??= GetComponent<LineRenderer>();

    private void Start()
    {
        GameController.Instance.eventsDispatcher.Register<CardDragStarted>(this, OnCardDragStarted);
        GameController.Instance.eventsDispatcher.Register<CardDragEnded>(this, OnCardDragEnded);
    }

    private void OnCardDragEnded(CardDragEnded cardDragEnded)
    {
        LineRenderer.enabled = false;
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

        LineRenderer.positionCount = positions.Length;
        LineRenderer.SetPositions(positions);
        LineRenderer.enabled = true;
    }

    private void OnDestroy()
    {
        GameController.Instance.eventsDispatcher.Unregister<CardDragStarted>(this);
        GameController.Instance.eventsDispatcher.Unregister<CardDragEnded>(this);
    }
}
