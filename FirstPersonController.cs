using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 10.0f;
    [SerializeField] private float walkSpeed = 2.0f; 
    
    private Camera _camera;
    private Rigidbody _rigidbody;
    
    private Vector2 _feetMoving;
    private Vector2 _cameraMoving;
    
    void Start()
    {
        _camera = FindOrCreateCamera();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private Camera FindOrCreateCamera()
    {
        foreach (var camera in GetComponentsInChildren<Camera>())
        {
            return camera;
        }
        
        var gameObject = new GameObject("PlayerCamera");
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = Vector3.zero;
        return gameObject.AddComponent<Camera>();
    }

    void Update()
    {
        var cameraMoving = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (IsSignificant(cameraMoving))
            cameraMoving.Normalize();
        else cameraMoving = Vector2.zero;
        
        var feetMoving = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (IsSignificant(feetMoving))
            feetMoving.Normalize();
        else feetMoving = Vector2.zero;
        
        _cameraMoving = cameraMoving;
        _feetMoving = feetMoving;
    }

    void FixedUpdate()
    {
        var feetMoving = _feetMoving;
        var currentVelocityY = _rigidbody.velocity.y;
        _rigidbody.velocity = transform.localToWorldMatrix * new Vector3(
            feetMoving.x * walkSpeed, currentVelocityY, 
            feetMoving.y * walkSpeed
        );

        var cameraMoving = _cameraMoving;
        if (IsSignificant(cameraMoving.x))
            transform.Rotate(Vector3.up, cameraMoving.x * mouseSensitivity);

        if (IsSignificant(cameraMoving.y))
        {
            var previousRotation = _camera.transform.localRotation;
            var deltaRotation = Quaternion.AngleAxis(cameraMoving.y * mouseSensitivity, Vector3.left);
            var newRotation = deltaRotation * previousRotation;
            
            var previousForward = previousRotation * Vector3.forward;
            var newForward = newRotation * Vector3.forward;

            if (Sign(cameraMoving.y) == Sign(Vector3.Dot(Vector3.up, newForward) - Vector3.Dot(Vector3.up, previousForward)))
            {
                _camera.transform.localRotation = newRotation;
            }
        }
    }

    private const float AxisThreshold = 0.001f;

    private static bool IsSignificant(in Vector2 vector)
    {
        return IsSignificant(vector.x) || IsSignificant(vector.y);
    }

    private static bool IsSignificant(float value)
    {
        return Mathf.Abs(value) >= AxisThreshold;
    }

    private static int Sign(float value)
    {
        if (value < -float.Epsilon) return -1;
        return value > float.Epsilon ? 1 : 0;
    }
}
