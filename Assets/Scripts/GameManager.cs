using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using System.ComponentModel;
using Enum;
using TMPro;
using Random = UnityEngine.Random;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once UnusedType.Global
    internal class IsExternalInit{}
}

public class GameManager : MonoBehaviour
{
    [FormerlySerializedAs("spheres")] [SerializeField] private List<GameObject> spheresToSpawn = new();
    [SerializeField] private Vector3 spawnAreaCenter = new(0, 20, 0);
    [SerializeField] private float spawnAreaRadius = 25;
    [SerializeField] private float doNotSpawnAreaRadius = 9;
    
    [SerializeField] private TextMeshProUGUI journal;
    [SerializeField] private float journalEntryLifetime = 5;

    private int _coins;
    private readonly List<JournalEntry> _journal = new();
    
    // Start is called before the first frame update
    private void Start()
    {
        _coins = 0;
        StartCoroutine(ScheduleSpawn());
    }

    private IEnumerator ScheduleSpawn()
    {
        while (true)
        {
            SpawnSphere();
            yield return new WaitForSeconds(5);
        }
    }

    public void Reward(int coins, SphereType color)
    {
        _coins += coins;
        StartCoroutine(AddEntryToJournal(JournalEntryType.Reward, color, color, coins));
    }

    public void Penalize(int coins, SphereType color, SphereType boxColor)
    {
        _coins -= coins;
        StartCoroutine(AddEntryToJournal(JournalEntryType.Penalty, color, boxColor, -coins));
    }

    private IEnumerator AddEntryToJournal(JournalEntryType type, SphereType color, SphereType boxColor, int coins)
    {
        var entry = new JournalEntry(type, color, boxColor, coins);
        _journal.Add(entry);
        PrintJournal();
        
        yield return new WaitForSeconds(journalEntryLifetime);
        
        _journal.Remove(entry);
        PrintJournal();
    }

    private void PrintJournal()
    {
        var journalText = $"Coins: {_coins}\n\n";
        foreach (var entry in _journal)
        {
            var sphereColorText = entry.SphereColor.ToString("G");
            var boxColorText = entry.BoxColor.ToString("G");
            journalText += $"{sphereColorText} sphere in the {boxColorText} box: {entry.Coins} coins\n";
        }
        journal.SetText(journalText);
    }

    private record JournalEntry(JournalEntryType Type, SphereType SphereColor, SphereType BoxColor, int Coins);

    private void SpawnSphere()
    {
        var dist = (spawnAreaRadius - doNotSpawnAreaRadius) * Random.value + doNotSpawnAreaRadius;
        var angle = Random.Range(0f, 360f);
        var location = Quaternion.Euler(0, angle, 0) * Vector3.forward * dist + spawnAreaCenter;
        var sphereToSpawn = spheresToSpawn[Random.Range(0, spheresToSpawn.Count)];
        Instantiate(sphereToSpawn, location, new Quaternion());
    }
}
