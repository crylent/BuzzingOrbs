using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private float movementForce;
    [SerializeField] private float slowMovementForceFactor = 0.5f;
    [SerializeField] private float camSensitivity = 1;
    [SerializeField] private float takeSphereDistance = 2;
    [SerializeField] private Vector3 holdVector = new(0, 0, 2f);
    [SerializeField] private Image forceIndicator;
    [SerializeField] private float minForce = 5f;
    [SerializeField] private float maxForce = 100f;
    [SerializeField] private float maxAccumulationTime = 2;
    [SerializeField] private float accumulationCurvePow = 2f;

    private GameManager _gameManager;
    private Rigidbody _rigidbody;
    private AudioSource _steps;
    private Vector3 _movement;
    private float _deltaX;
    private float _deltaY;
    private Outline _activeOutline;
    
    // Start is called before the first frame update
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _rigidbody = GetComponent<Rigidbody>();
        _steps = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!_gameManager.IsGameActive) return;

        _rigidbody.AddRelativeForce(movementForce * (_takenSphere ? slowMovementForceFactor : 1) * _movement);
    }

    [SerializeField] private float cameraMaxPitch = 40;
    [SerializeField] private float cameraMinPitch = -90;
    
    private void LateUpdate()
    {
        if (!_gameManager.IsGameActive) return;
        
        transform.Rotate(0, _deltaX * camSensitivity, 0);
        var rot = playerCam.transform.eulerAngles;
        if (rot.x > 180) rot.x -= 360;
        rot.x = Math.Clamp(rot.x + _deltaY * camSensitivity, cameraMinPitch, cameraMaxPitch);
        playerCam.transform.eulerAngles = rot;

        var ray = GetRayFromEyes();
        if (Physics.Raycast(ray, out var hit, takeSphereDistance) && hit.collider.CompareTag("Sphere"))
        {
            _activeOutline = hit.collider.gameObject.GetComponent<Outline>();
            _activeOutline.enabled = true;
        }
        else if (_activeOutline)
        {
            _activeOutline.enabled = false;
            _activeOutline = null;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!_gameManager.IsGameActive)
        {
            _movement = Vector3.zero;
            _steps.Stop();
            return;
        }

        var movement = context.ReadValue<Vector2>().normalized;
        _movement = new Vector3(movement.x, 0, movement.y);

        if (context.started)
        {
            _steps.Play();
        }
        else if (context.canceled)
        {
            _steps.Stop();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!_gameManager.IsGameActive) return;

        var delta = context.ReadValue<Vector2>();
        _deltaX = delta.x;
        _deltaY = -delta.y;
    }

    private GameObject _takenSphere;
    private float _accumulationTime;

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!_gameManager.IsGameActive) return;

        if (!_activeOutline && !_takenSphere) return;

        var sphere = _takenSphere ? _takenSphere : _activeOutline.gameObject;
        
        var sphereRigidbody = sphere.GetComponent<Rigidbody>();
        var sphereTransform = sphere.transform;
        if (!_takenSphere && context.canceled) // take sphere and attack to player
        {
            sphereTransform.SetParent(playerCam.transform);
            sphereTransform.localPosition = holdVector;
            sphereRigidbody.isKinematic = true;
            _takenSphere = sphere;
            _steps.pitch = 0.75f;
        }
        else if (_takenSphere && context.started) // accumulate force
        {
            StartCoroutine(AccumulateForce());
        }
        else if (_takenSphere && context.canceled) // detach sphere from player and throw
        {
            sphereTransform.SetParent(null);
            sphereRigidbody.isKinematic = false;
            var accumulation = Mathf.Pow(_accumulationTime / maxAccumulationTime, accumulationCurvePow);
            var force = (maxForce - minForce) * accumulation + minForce;
            sphereRigidbody.AddForce(GetRayFromEyes().direction * force, ForceMode.Impulse);
            sphereRigidbody.drag = 0;
            _takenSphere = null;
            forceIndicator.fillAmount = 0;
            _steps.pitch = 1f;
        }
    }

    private IEnumerator AccumulateForce()
    {
        _accumulationTime = 0f;
        while (_accumulationTime < maxAccumulationTime && _takenSphere)
        {
            _accumulationTime += Time.fixedDeltaTime;
            forceIndicator.fillAmount = _accumulationTime / maxAccumulationTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private Ray GetRayFromEyes()
    {
        return playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    }
}
