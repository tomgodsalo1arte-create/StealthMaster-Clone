using System;
using UnityEngine;

public class CCTvFieldOfView : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] public float viewDistance = 5f;
    [SerializeField] private int rayCount = 25;
    public float fov = 90f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;

    public event Action<Transform> OnTargetSeen;
    public event Action OnTargetLost;
    /// <summary>
    /// ///////////////////////////////
    /// </summary>
    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoRayColor = Color.yellow;
    [SerializeField] private Color gizmoHitColor = Color.red;
    [SerializeField] private Color gizmoFovEdgeColor = Color.cyan;

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Material materialInstance;

    private Vector3 origin;
    private float startingAngle;
    private bool targetVisible;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        materialInstance = meshRenderer.material;
        materialInstance.color = normalColor;

    }

    private void LateUpdate()
    {
        DrawFOV();
        DetectTarget();
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (PlayerBehaviour.Instance == null) return;

        Vector3 gizmoOrigin = origin != Vector3.zero ? origin : transform.position;
        Vector3 playerPos = PlayerBehaviour.Instance.transform.position;
        Vector3 dirToPlayer = (playerPos - gizmoOrigin).normalized;

        // Draw direction to player
        Gizmos.color = gizmoRayColor;
        Gizmos.DrawRay(gizmoOrigin, dirToPlayer * viewDistance);

        // Draw hit point if ray hits something
        if (Physics.Raycast(gizmoOrigin, dirToPlayer, out RaycastHit hit, viewDistance, detectionMask))
        {
            Gizmos.color = hit.collider.CompareTag("Player") ? gizmoHitColor : Color.gray;
            Gizmos.DrawSphere(hit.point, 0.1f);
        }

        // Draw FOV edges
        Gizmos.color = gizmoFovEdgeColor;

        Vector3 forward = transform.forward;
        float halfFov = fov * 0.5f;

        Quaternion leftRot = Quaternion.AngleAxis(-halfFov, transform.up);
        Quaternion rightRot = Quaternion.AngleAxis(halfFov, transform.up);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawRay(gizmoOrigin, leftDir * viewDistance);
        Gizmos.DrawRay(gizmoOrigin, rightDir * viewDistance);
        int steps = 20;
        float stepAngle = fov / steps;

        for (int i = 0; i <= steps; i++)
        {
            float angle = -halfFov + stepAngle * i;
            Vector3 dir = Quaternion.AngleAxis(angle, transform.up) * forward;
            Gizmos.DrawRay(gizmoOrigin, dir * viewDistance);
        }

    }

    // =========================
    // DETECTION
    // =========================
    private void DetectTarget()
    {
        if (PlayerBehaviour.Instance == null) return;

        Vector3 playerPos = PlayerBehaviour.Instance.transform.position;
        Vector3 dirToPlayer = (playerPos - origin).normalized;

        Vector3 forward = transform.forward;
        float angleToPlayer = Vector3.Angle(forward, dirToPlayer);

        if (angleToPlayer > fov * 0.5f)
        {
            LoseTarget();
            return;
        }

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, viewDistance, detectionMask))
        {
            
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawRay(origin, dirToPlayer * viewDistance, Color.red);

                if (!targetVisible)
                {
                    targetVisible = true;
                    OnTargetSeen?.Invoke(hit.transform);
                }
                return;
            }
        }

        LoseTarget();
    }



    private void LoseTarget()
    {
        if (!targetVisible) return;
        targetVisible = false;
        OnTargetLost?.Invoke();
    }

    // =========================
    // FOV VISUAL
    // =========================
    private void DrawFOV()
    {
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 dir = GetVectorFromAngle(angle);
            Vector3 vertex;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, detectionMask))
                vertex = hit.point;
            else
                vertex = origin + dir * viewDistance;

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = vertexIndex - 1;
                triangles[triangleIndex++] = vertexIndex;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
    }

    // =========================
    // PUBLIC API
    // =========================
    public void SetOrigin(Vector3 origin) => this.origin = origin;

    public void SetAimDirection(Vector3 dir)
    {
        startingAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg + fov * 0.5f;
    }

    public void SetAlert(bool alert)
    {
        if (materialInstance == null) return;
        materialInstance.color = alert ? alertColor : normalColor;
    }
}
