using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{    
    public Transform playerTransform;
    public Vector3 orbitOffset = Vector3.zero; // Offset for camera orbit around player
    public float distanceFromPlayer = 10f;
    public float maxTopAngle = 80f;
    public float maxBottomAngle = 20f;
    public float mouseSensitivity = 5f;
    public float wasdSensitivity = 1f;
    public float smoothTime = 3f; // Increased for even more gradual following to handle 0.3s jumps
    public float zoomSpeed = 2f;
    public float minDistance = 5f;
    public float maxDistance = 20f;
    public float cameraSmoothTime = 0.5f; // Slightly slower camera movement
    public Volume globalVolume;
    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 smoothedTargetPosition;
    private Vector3 targetVelocity = Vector3.zero;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void OnChunkLoaded(Vector2Int @int, GameObject player)
    {
        if (playerTransform == null)
        {
            playerTransform = player.transform;
            InitializeCamera();
        }
    }

    void InitializeCamera()
    {
        currentX = transform.eulerAngles.y;
        currentY = transform.eulerAngles.x;
        smoothedTargetPosition = playerTransform.position;
        
        // Instantly position camera without smoothing (like LateUpdate but immediate)
    Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
    Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
    Vector3 desiredPosition = smoothedTargetPosition + rotation * direction + orbitOffset;
        
    // Set camera position instantly instead of smoothing
    transform.position = desiredPosition;
    transform.LookAt(smoothedTargetPosition + orbitOffset);
        
    // Reset velocity to prevent smoothing artifacsts
    velocity = Vector3.zero;
    targetVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (Input.GetMouseButton(2))
        {
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, -maxBottomAngle, maxTopAngle);
        }

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        currentX += horizontal * wasdSensitivity * Time.deltaTime * 50f;
        currentY -= vertical * wasdSensitivity * Time.deltaTime * 50f;
        currentY = Mathf.Clamp(currentY, -maxBottomAngle, maxTopAngle);

        // Check if mouse is over UI
        PointerEventData zoomPointerData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> zoomResults = new();
        EventSystem.current.RaycastAll(zoomPointerData, zoomResults);
        bool isOverUI = zoomResults.Count > 0;
        bool allowed = true;
        distanceFromPlayer -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distanceFromPlayer = Mathf.Clamp(distanceFromPlayer, minDistance, maxDistance);
    }

    private void LateUpdate()
    {
        if (!playerTransform) return;

        // Smooth the target position to reduce jerkiness from player snapping
        smoothedTargetPosition = Vector3.SmoothDamp(smoothedTargetPosition, playerTransform.position, ref targetVelocity, smoothTime);

    Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
    Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
    Vector3 desiredPosition = smoothedTargetPosition + rotation * direction + orbitOffset;
        
    // Smooth camera position to desired position
    transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, cameraSmoothTime);
        
    // Instantly look at the smoothed target
    transform.LookAt(smoothedTargetPosition + orbitOffset);
    }
}