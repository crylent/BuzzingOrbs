using System;
using System.Collections;
using Enum;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    [SerializeField] private Material transparent;
    [SerializeField] private Material black;
    [SerializeField] private Material red;
    [SerializeField] private Material green;
    [SerializeField] private Material blue;
    [SerializeField] private Material yellow;
    [SerializeField] private Material purple;
    [SerializeField] private float flashTime = 0.3f;

    private MeshRenderer _renderer;
    private Light _light;
    private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

    // Start is called before the first frame update
    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _light = GetComponent<Light>();
        SetColor(null);
    }

    public IEnumerator Flash(SphereType color)
    {
        SetColor(color);
        yield return new WaitForSeconds(flashTime);
        SetColor(null);
    }

    private void SetColor(SphereType? color)
    {
        _renderer.material = color switch
        {
            null => transparent,
            SphereType.Black => black,
            SphereType.Red => red,
            SphereType.Green => green,
            SphereType.Blue => blue,
            SphereType.Yellow => yellow,
            SphereType.Purple => purple,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
        if (color == null)
        {
            _light.enabled = false;
        }
        else
        {
            _light.enabled = true;
            _light.color = _renderer.material.GetColor(Emission);
        }
    }
}
