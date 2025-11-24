using UnityEngine;

public class SmoothOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float radius = 10f;
    public float minRadius = 2f;
    public float maxRadius = 50f;

    [Header("Orbit Settings")]
    public float rotationSpeed = 120f;
    public float rotationDamping = 5f;
    public float zoomDamping = 8f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;

    private float yaw;
    private float pitch;
    private float targetYaw;
    private float targetPitch;
    private float targetRadius;
    private Vector3 currentVelocity;

    private Vector2 lastTouchPos;
    private bool isTouching;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (target == null)
        {
            Debug.LogWarning("No target assigned. Creating dummy target at origin.");
            GameObject dummy = new GameObject("DummyTarget");
            target = dummy.transform;
        }

        Vector3 offset = transform.position - target.position;
        targetRadius = radius = offset.magnitude;

        Quaternion rot = Quaternion.LookRotation(-offset, Vector3.up);
        targetYaw = yaw = rot.eulerAngles.y;
        targetPitch = pitch = rot.eulerAngles.x;
    }

    private void LateUpdate()
    {
        HandleInput();

        // Smoothly interpolate rotation and radius
        yaw = Mathf.Lerp(yaw, targetYaw, Time.deltaTime * rotationDamping);
        pitch = Mathf.Lerp(pitch, targetPitch, Time.deltaTime * rotationDamping);
        radius = Mathf.Lerp(radius, targetRadius, Time.deltaTime * zoomDamping);

        // Calculate new position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * radius;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 0.08f);
        transform.LookAt(target);
    }

    private void HandleInput()
    {
        // ===== Mouse Rotation =====
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            targetYaw += mouseX * rotationSpeed * Time.deltaTime;
            targetPitch -= mouseY * rotationSpeed * Time.deltaTime;
        }

        // ===== Scroll Wheel Zoom =====
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetRadius -= scroll * radius * 0.5f;
        }

        // ===== Touch Controls =====
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPos = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                Vector2 delta = touch.deltaPosition;
                targetYaw += delta.x * rotationSpeed * 0.02f * Time.deltaTime;
                targetPitch -= delta.y * rotationSpeed * 0.02f * Time.deltaTime;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isTouching = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevMag = (prev0 - prev1).magnitude;
            float currMag = (t0.position - t1.position).magnitude;

            float diff = currMag - prevMag;
            targetRadius -= diff * 0.01f;
        }

        // Clamp pitch and radius
        targetPitch = Mathf.Clamp(targetPitch, pitchMin, pitchMax);
        targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
    }
}
