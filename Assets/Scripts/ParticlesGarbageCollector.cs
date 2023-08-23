using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlesGarbageCollector : MonoBehaviour
{
    private readonly List<ParticleSystem> _systems = new();

    // Update is called once per frame
    private void Update()
    {
        List<ParticleSystem> toRemove = new();
        foreach (var system in _systems.Where(system => !system.IsAlive()))
        {
            Destroy(system.gameObject);
            toRemove.Add(system);
        }
        foreach (var system in toRemove)
        {
            _systems.Remove(system);
        }
    }

    public void RegisterSystem(ParticleSystem system)
    {
        _systems.Add(system);
    }
}
