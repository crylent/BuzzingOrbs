using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    [FormerlySerializedAs("CounterText")] public Text counterText;

    private int _count = 0;

    private void Start()
    {
        _count = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        _count += 1;
        counterText.text = "Count : " + _count;
    }
}