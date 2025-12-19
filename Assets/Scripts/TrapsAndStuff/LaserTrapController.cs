using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTrapController : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private float laserDistance = 10f;
    [SerializeField] private LayerMask hitMask;

    [Header("Movement")]
    [SerializeField] private bool rotateLaser = true;
    [SerializeField] private float rotationSpeed = 30f;
    
    private LineRenderer lineRenderer;
    public static event Action OnLaserHit;
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        //HandleMovement();
        DrawLaser();
        DetectHit();
    }

    private void HandleMovement()
    {
        if (!rotateLaser) return;

        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void DrawLaser()
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.up;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start + direction * laserDistance);
    }

    private void DetectHit()
    {
        Ray ray = new Ray(transform.position, transform.up);

        if (Physics.Raycast(ray, out RaycastHit hit, laserDistance, hitMask))
        {
            lineRenderer.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Player"))
            {
                OnPlayerHit();
            }
        }
    }

    private void OnPlayerHit()
    {
        
        OnLaserHit?.Invoke();
        // Options:
        // - Kill player
        // - Trigger alarm
        // - Alert enemies
    }
}
