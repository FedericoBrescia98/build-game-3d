using System.Numerics;
using Cinemachine;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class CameraController : MonoBehaviour
{
    [SerializeField] private bool _useEdgeScrolling = true;
    [SerializeField] private bool _useDragRotation = true;
    [SerializeField] private CinemachineVirtualCamera _mainCamera;

    // nell'oggetto da seguire: CameraController.instance.followTransform = transform;
    [SerializeField] private Transform _followTransform;
    private float _moveSpeed;

    [SerializeField] private float _moveTime = 5;
    [SerializeField] private float _normalSpeed = 0.2f;
    [SerializeField] private float _fastSpeed = 1;
    [SerializeField] private float _rotationAmount = 0.2f;
    [SerializeField] private float _zoomAmount = 10;
    [SerializeField] private float _maxZoom = 80;
    [SerializeField] private float _minZoom = 10;
    [SerializeField] private float _minXPosition = -20;
    [SerializeField] private float _maxXPosition = 320;
    [SerializeField] private float _minZPosition = -20;
    [SerializeField] private float _maxZPosition = 320;

    private Vector3 _newPosition;
    private Quaternion _newRotation;
    private float _newZoom = 60;

    private PlayerInputActions _playerInputActions;

    [SerializeField] private int _edgeScrollSize = 20;

    [SerializeField] private AnimationCurve _mouseSensitivityCurve =
        new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Camera.Enable();
    }

    private void Start()
    {
        _newPosition = transform.position;
        _newRotation = transform.rotation;
    }

    private void Update()
    {
        if (_followTransform != null)
        {
            FollowTransform();
        }
        else
        {
            HandleCameraMovement();
            HandleCameraRotation();
            HandleCameraZoom();
        }
    }

    private void FollowTransform()
    {
        transform.position = _followTransform.position;
       if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _followTransform = null;
        }
    }

    private void HandleCameraMovement()
    {
        _moveSpeed = Input.GetKey(KeyCode.LeftShift) ? _fastSpeed : _normalSpeed;

        if (_useEdgeScrolling)
        {
            _newPosition += HandleCameraEdgeScrolling();
        }

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            _newPosition += (transform.forward * _moveSpeed);
        }
        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            _newPosition += (transform.forward * -_moveSpeed);
        }
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _newPosition += (transform.right * -_moveSpeed);
        }
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _newPosition += (transform.right * _moveSpeed);
        }

        _newPosition.x = Mathf.Clamp(_newPosition.x, _minXPosition, _maxXPosition);
        _newPosition.z = Mathf.Clamp(_newPosition.z, _minZPosition, _maxZPosition);
        transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * _moveTime);
    }

    private Vector3 HandleCameraEdgeScrolling()
    {
        Vector3 newPosition = Vector3.zero;
        if(Input.mousePosition.y > Screen.height - _edgeScrollSize)
        {
            newPosition += (transform.forward * _moveSpeed);
        }

        if(Input.mousePosition.y < _edgeScrollSize)
        {
            newPosition += (transform.forward * -_moveSpeed);
        }

        if(Input.mousePosition.x < _edgeScrollSize)
        {
            newPosition += (transform.right * -_moveSpeed);
        }

        if(Input.mousePosition.x > Screen.width - _edgeScrollSize)
        {
            newPosition += (transform.right * _moveSpeed);
        }
        return newPosition;
    }

    private void HandleCameraRotation()
    {
        if(Input.GetKey(KeyCode.Q))
        {
            _newRotation *= Quaternion.Euler(Vector3.up * _rotationAmount);
        }
        if(Input.GetKey(KeyCode.E))
        {
            _newRotation *= Quaternion.Euler(Vector3.up * -_rotationAmount);
        }

        if (Input.GetMouseButton(2) && _useDragRotation)
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            var mouseSensitivityFactor = _mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
            _newRotation *= Quaternion.Euler(mouseMovement.x * mouseSensitivityFactor * Vector3.up);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, _newRotation, Time.deltaTime * _moveTime);
    }

    private void HandleCameraZoom()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _newZoom += _zoomAmount;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            _newZoom += -_zoomAmount;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            _newZoom += 20 * Input.GetAxis("Mouse ScrollWheel") * -_zoomAmount;
        }
            
        _newZoom = Mathf.Clamp(_newZoom, _minZoom, _maxZoom);
        _mainCamera.m_Lens.FieldOfView = Mathf.Lerp(_mainCamera.m_Lens.FieldOfView, _newZoom, Time.deltaTime * _moveTime);
    }
}
