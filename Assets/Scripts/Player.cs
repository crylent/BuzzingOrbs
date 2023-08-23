using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private float movementForce;
    [SerializeField] private float slowMovementForceFactor = 0.5f;
    [SerializeField] private float camSensitivity = 1;
    [SerializeField] private float holdSphereDistance = 2;
    [SerializeField] private Vector3 holdVector = new(0, 0, 2f);

    private Rigidbody _rigidbody;
    private Vector3 _movement;
    private float _deltaX;
    private float _deltaY;
    private Outline _activeOutline;
    
    // Start is called before the first frame update
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _rigidbody.AddRelativeForce(movementForce * (_takenSphere ? slowMovementForceFactor : 1) * _movement);
    }

    [SerializeField] private float cameraMaxPitch = 40;
    [SerializeField] private float cameraMinPitch = -90;
    
    private void LateUpdate()
    {
        transform.Rotate(0, _deltaX * camSensitivity, 0);
        var rot = playerCam.transform.eulerAngles;
        if (rot.x > 180) rot.x -= 360;
        rot.x = Math.Clamp(rot.x + _deltaY * camSensitivity, cameraMinPitch, cameraMaxPitch);
        playerCam.transform.eulerAngles = rot;

        var ray = GetRayFromEyes();
        if (Physics.Raycast(ray, out var hit, holdSphereDistance) && hit.collider.CompareTag("Sphere"))
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
        var movement = context.ReadValue<Vector2>().normalized;
        _movement = new Vector3(movement.x, 0, movement.y);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        var delta = context.ReadValue<Vector2>();
        _deltaX = delta.x;
        _deltaY = -delta.y;
    }

    private GameObject _takenSphere;

    public void OnClick(InputAction.CallbackContext context)
    {
        if ((!_activeOutline && !_takenSphere) || !context.performed) return;

        var sphere = _takenSphere ? _takenSphere : _activeOutline.gameObject;
        
        var sphereRigidbody = sphere.GetComponent<Rigidbody>();
        var sphereTransform = sphere.transform;
        if (!_takenSphere)
        {
            // attach to player
            sphereTransform.SetParent(playerCam.transform);
            sphereTransform.localPosition = holdVector;
            sphereRigidbody.isKinematic = true;
            _takenSphere = sphere;
        }
        else
        {
            // detach from player and throw
            sphereTransform.SetParent(null);
            sphereRigidbody.isKinematic = false;
            sphereRigidbody.AddForce(GetRayFromEyes().direction * 10, ForceMode.Impulse);
            sphereRigidbody.drag = 0;
            _takenSphere = null;
        }
    }

    private Ray GetRayFromEyes()
    {
        return playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    }
}
