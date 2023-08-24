using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using System.ComponentModel;
using System.Linq;
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
    [FormerlySerializedAs("spheres")] [SerializeField] private List<Sphere> spheresToSpawn = new();
    [SerializeField] private Vector3 spawnAreaCenter = new(0, 20, 0);
    [SerializeField] private float spawnAreaRadius = 25;
    [SerializeField] private float doNotSpawnAreaRadius = 9;

    [SerializeField] private float spawnPeriod = 5f;
    
    [SerializeField] private TextMeshProUGUI journal;
    [SerializeField] private float journalEntryLifetime = 5;

    [SerializeField] private List<AudioClip> successSounds = new();

    private int _coins;
    private readonly List<JournalEntry> _journal = new();
    private AudioSource _audio;
    
    // Start is called before the first frame update
    private void Start()
    {
        _coins = 0;
        _audio = GetComponent<AudioSource>();
        StartCoroutine(ScheduleSpawn());
    }

    private IEnumerator ScheduleSpawn()
    {
        while (true)
        {
            SpawnSphere();
            yield return new WaitForSeconds(spawnPeriod);
        }
    }

    public void Reward(int coins, SphereType color)
    {
        _coins += coins;
        _audio.PlayOneShot(successSounds[Random.Range(0, successSounds.Count)]);
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

        var factorsSum = spheresToSpawn.Sum(sphere => sphere.ChanceFactor);
        var rand = Random.value * factorsSum;

        var factorsCounter = 0f;
        foreach (var sphere in spheresToSpawn)
        {
            factorsCounter += sphere.ChanceFactor;
            if (rand > factorsCounter) continue;
            
            Instantiate(sphere.gameObject, location, new Quaternion());
            break;
        }
    }
}
