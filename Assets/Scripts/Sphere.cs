using System.Collections;
using Enum;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private SphereType type;
    public SphereType Type => type;
    [SerializeField] private int coins = 10;
    public int Coins => coins;
    [SerializeField] private float spawnChanceFactor = 1f;
    public float ChanceFactor => spawnChanceFactor;

    private GameManager _gameManager;
    private ParticlesGarbageCollector _collector;
    private AudioSource _audio;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _collector = FindObjectOfType<ParticlesGarbageCollector>();
        _audio = GetComponent<AudioSource>();
        StartCoroutine(DestroyOnGameFinished());
    }

    private void OnCollisionEnter(Collision collision)
    {
        var obj = collision.gameObject;
        if (obj.CompareTag("Player") || obj.CompareTag("Ground"))
        {
            Destroy(); // destroy sphere with particle effect
        }
    }

    private IEnumerator DestroyOnGameFinished()
    {
        yield return new WaitUntil(() => !_gameManager.IsGameActive);
        Destroy();
    }

    public void Destroy()
    {
        var system = Instantiate(destroyEffect, transform.position, new Quaternion());
        _collector.RegisterSystem(system);
        Destroy(gameObject);
    }

    public void DisableSound()
    {
        _audio.enabled = false;
    }
}
