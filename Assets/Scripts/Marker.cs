using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField] private float speed;

    public float Speed => speed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Collided with {other.gameObject.name}");
        if (other.TryGetComponent<Collectable>(out var collectable))
        {
            collectable.Collect();
        }
    }
}
