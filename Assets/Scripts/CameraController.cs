using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private bool useEdgeScrolling = true;
    [SerializeField] private bool useDragRotation = true;
    [SerializeField] private Camera mainCamera;

    // nell'oggetto da seguire: CameraController.instance.followTransform = transform;
    [SerializeField] private Transform followTransform;
    private float moveSpeed;

    [SerializeField] private float moveTime = 5;
    [SerializeField] private float normalSpeed = 0.2f;
    [SerializeField] private float fastSpeed = 1;
    [SerializeField] private float rotationAmount = 0.2f;
    [SerializeField] private float zoomAmount = 10;
    [SerializeField] private float maxZoom = 80;
    [SerializeField] private float minZoom = 10;

    private Vector3 newPosition;
    private Quaternion newRotation;
    private float newZoom = 20;

    [SerializeField] private int edgeScrollSize = 20;
    [SerializeField] private AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {

        if(followTransform != null)
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

    void FollowTransform()
    {
        transform.position = followTransform.position;
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            followTransform = null;
        }
    }

    void HandleCameraMovement()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = fastSpeed;
        }
        else
        {
            moveSpeed = normalSpeed;
        }
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || (useEdgeScrolling && Input.mousePosition.y > Screen.height - edgeScrollSize))
        {
            newPosition += (transform.forward * moveSpeed);
        }
        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || (useEdgeScrolling && Input.mousePosition.y < edgeScrollSize))
        {
            newPosition += (transform.forward * -moveSpeed);
        }
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || (useEdgeScrolling && Input.mousePosition.x < edgeScrollSize))
        {
            newPosition += (transform.right * -moveSpeed);
        }
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (useEdgeScrolling && Input.mousePosition.x > Screen.width - edgeScrollSize))
        {
            newPosition += (transform.right * moveSpeed);
        }
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * moveTime);
    }

    void HandleCameraRotation()
    {
        if(Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if(Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
        if(Input.GetMouseButton(2) && useDragRotation)
        {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
            newRotation *= Quaternion.Euler(mouseMovement.x * mouseSensitivityFactor * Vector3.up);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * moveTime);
    }

    void HandleCameraZoom()
    {
        if(Input.GetKey(KeyCode.T))
        {
            newZoom += zoomAmount;
        }
        if(Input.GetKey(KeyCode.G))
        {
            newZoom += -zoomAmount;
        }
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            newZoom += 20 * Input.GetAxis("Mouse ScrollWheel") * -zoomAmount;
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        }
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, newZoom, Time.deltaTime * moveTime);
    }

    //void ClipCheck()
    //{
    //    Ray ray = new Ray(parentObject.position, transform.forward);
    //    if(Physics.SphereCast(ray, 3, out RaycastHit hit, maxZoom))
    //    {
    //        if(hit.distance < newZoom + 3)
    //        {
    //            newZoom = hit.distance - 3;
    //        }
    //    }
    //}
}
