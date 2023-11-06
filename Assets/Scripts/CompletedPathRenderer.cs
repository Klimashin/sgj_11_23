using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CompletedPathRenderer : MonoBehaviour
{
    [SerializeField] private Marker marker;
    [SerializeField] private float updateCooldown = 0.1f;
    
    private LineRenderer _lineRenderer;
    private LineRenderer LineRenderer => _lineRenderer ??= GetComponent<LineRenderer>();

    private readonly List<Vector3> _positions = new ();
    private float _currentUpdateCooldown = 0f;

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
            LineRenderer.positionCount = _positions.Count;
            LineRenderer.SetPositions(_positions.ToArray());
            _currentUpdateCooldown = updateCooldown;
        }
    }
}
