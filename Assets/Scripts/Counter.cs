using Enum;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] private SphereType acceptableType;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var sphere = other.GetComponent<Sphere>();
        var coins = sphere.Coins;
        
        if (acceptableType == sphere.Type)
        {
            _gameManager.Reward(coins, sphere.Type);
        }
        else
        {
            sphere.Destroy();
            _gameManager.Penalize(coins, sphere.Type, acceptableType);
        }
    }
}