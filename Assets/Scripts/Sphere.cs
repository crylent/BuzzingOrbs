using UnityEngine;

public class Sphere : MonoBehaviour
{
    [SerializeField] private ParticleSystem destroyEffect;

    private ParticlesGarbageCollector _collector;

    private bool _isFragile;

    private void Start()
    {
        _collector = FindObjectOfType<ParticlesGarbageCollector>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") &&
            (!collision.gameObject.CompareTag("Ground") || !_isFragile)) return;
        
        var system = Instantiate(destroyEffect, transform.position, new Quaternion());
        _collector.RegisterSystem(system);
        Destroy(gameObject);
    }

    public void MakeFragile()
    {
        _isFragile = true;
    }
}
