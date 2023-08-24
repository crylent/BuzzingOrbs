using System;
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
    [SerializeField] private float gameTime = 300f;

    private float _timeRemaining;

    [SerializeField] private List<AudioClip> successSounds = new();
    [SerializeField] private Lamp lamp;
    
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private int countdownTimes = 10;
    [SerializeField] private float countdownPeriod = 1f;

    [SerializeField] private GameObject menu;
    [SerializeField] private TextMeshProUGUI info;
    
    [SerializeField] private GameObject hud;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI journal;
    [SerializeField] private float journalEntryLifetime = 5;

    public bool IsGameActive { get; private set; }
    private int _coins;
    private int _coinsRecord;
    private readonly List<JournalEntry> _journal = new();
    private AudioSource _audio;
    
    // Start is called before the first frame update
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!IsGameActive) return;
        
        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining < countdownTimes * countdownPeriod && !_countdownIsActive)
        {
            StartCoroutine(Countdown());
        }
        else if (_timeRemaining < 0)
        {
            EndGame();
            return;
        }
        var time = TimeSpan.FromSeconds(_timeRemaining);
        timer.SetText($"{time.Minutes:D1}:{time.Seconds:D2}.{time.Milliseconds:D3}");
    }

    private bool _countdownIsActive;
    
    private IEnumerator Countdown()
    {
        _countdownIsActive = true;
        for (var i = 0; i < countdownTimes; i++)
        {
            _audio.PlayOneShot(countdownSound);
            yield return new WaitForSeconds(countdownPeriod);
        }
        _countdownIsActive = false;
    }

    public void StartGame()
    {
        IsGameActive = true;
        hud.SetActive(true);
        menu.SetActive(false);
        _timeRemaining = gameTime;
        _coins = 0;
        StartCoroutine(ScheduleSpawn());
    }

    private void EndGame()
    {
        IsGameActive = false;
        hud.SetActive(false);
        menu.SetActive(true);
        if (_coins > _coinsRecord)
        {
            _coinsRecord = _coins;
        }
        
        info.SetText($"Last Result:  {_coins} coins\nRecord: {_coinsRecord} coins");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator ScheduleSpawn()
    {
        while (IsGameActive)
        {
            SpawnSphere();
            yield return new WaitForSeconds(spawnPeriod);
        }
    }

    public void Reward(int coins, SphereType color)
    {
        _coins += coins;
        _audio.PlayOneShot(successSounds[Random.Range(0, successSounds.Count)]);
        StartCoroutine(lamp.Flash(color));
        StartCoroutine(AddEntryToJournal(color, color, coins));
    }

    public void Penalize(int coins, SphereType color, SphereType boxColor)
    {
        _coins -= coins;
        StartCoroutine(AddEntryToJournal(color, boxColor, -coins));
    }

    private IEnumerator AddEntryToJournal(SphereType color, SphereType boxColor, int coins)
    {
        var entry = new JournalEntry(color, boxColor, coins);
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

    private record JournalEntry(SphereType SphereColor, SphereType BoxColor, int Coins);

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
