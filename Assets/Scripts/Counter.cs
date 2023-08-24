using System;
using Enum;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] private SphereType acceptableType;
    [SerializeField] private int penalty = 10;

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
            sphere.DisableSound();
            _gameManager.Reward(coins, sphere.Type);
        }
        else
        {
            sphere.Destroy();
            _gameManager.Penalize(penalty, sphere.Type, acceptableType);
        }
    }
}