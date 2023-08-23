using Enum;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private SphereType type;
    public SphereType Type => type;
    [SerializeField] private int coins = 10;
    public int Coins => coins;

    private ParticlesGarbageCollector _collector;

    private void Start()
    {
        _collector = FindObjectOfType<ParticlesGarbageCollector>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var obj = collision.gameObject;
        if (obj.CompareTag("Player") || obj.CompareTag("Ground"))
        {
            Destroy(); // destroy sphere with particle effect
        }
    }

    public void Destroy()
    {
        var system = Instantiate(destroyEffect, transform.position, new Quaternion());
        _collector.RegisterSystem(system);
        Destroy(gameObject);
    }
}
