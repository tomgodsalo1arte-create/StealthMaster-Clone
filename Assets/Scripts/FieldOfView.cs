using System;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] public float viewDistance = 5f;
    [SerializeField] private int rayCount = 25;
    public float fov = 90f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color alertColor = Color.red;
    [Header("Mode")]
[SerializeField] private bool isCCTV = false;

    [Header("CCTV Spotlight")]
    [SerializeField] private float spotlightRadius = 1.5f;
    [SerializeField] private int spotlightSegments = 24;
    [SerializeField] private float groundOffset = 0.02f;
    [SerializeField] private Transform camDirection;
    public event Action<Transform> OnTargetSeen;
    public event Action OnTargetLost;

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
     
        DetectTarget();
    }

    // =========================
    // DETECTION
    // =========================
    private void DetectTarget()
    {
        if (PlayerBehaviour.Instance == null)
            return;

        if (isCCTV)
        {
            DetectCCTV();
            DrawSpotlight();
        }
        else
        {
            DrawFOV();
            DetectNormal();
        }
    }

    private void DetectCCTV()
    {
        if (PlayerBehaviour.Instance == null)
            return;

        // 1️⃣ Ray direction = CCTV local Z axis (follows Y rotation)
        Vector3 forward = transform.forward;

        // 2️⃣ Optional FOV angle check (keeps cone logic consistent)
        Vector3 toPlayer = (PlayerBehaviour.Instance.transform.position - origin).normalized;
        float angleToPlayer = Vector3.Angle(forward, toPlayer);

        if (angleToPlayer > fov * 0.5f)
        {
            LoseTarget();
            return;
        }

        // 3️⃣ Raycast STRAIGHT forward from origin
        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, viewDistance, detectionMask))
        {
            Debug.DrawRay(origin, forward * viewDistance, Color.red);

            if (hit.collider.CompareTag("Player"))
            {
                if (!targetVisible)
                {
                    targetVisible = true;
                    OnTargetSeen?.Invoke(hit.transform);
                }
                return;
            }
        }

        // 4️⃣ Nothing detected
        LoseTarget();
    }


    private void DetectNormal()
    {
        //if (PlayerBehaviour.Instance == null) return;
        
        Vector3 playerPos = PlayerBehaviour.Instance.transform.position;
        Vector3 dirToPlayer = (playerPos - origin).normalized;
        float angleToPlayer = Vector3.Angle(GetVectorFromAngle(startingAngle - fov / 2f), dirToPlayer);

        if (angleToPlayer > fov * 0.5f)
        {
          //  Debug.Log("SEET TO LoseTarget");
            LoseTarget();
            return;
        }

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, viewDistance, detectionMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("SEET TO hit.collider.CompareTag(\"Player\")");
                if (!targetVisible)
                {
                    Debug.Log("SEET TO !targetVisible");
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
                Debug.DrawRay(origin, dir * viewDistance, Color.red);// 

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
    private void DrawSpotlight()
    {
        // Position at the end of the FOV
        Vector3 forward = transform.forward;
        Vector3 center = origin + forward * viewDistance;

        // Project onto ground
        center.y = origin.y + groundOffset;

        Vector3[] vertices = new Vector3[spotlightSegments + 1];
        int[] triangles = new int[spotlightSegments * 3];

        vertices[0] = center; // center of the circle

        float angleStep = 360f / spotlightSegments;

        for (int i = 0; i < spotlightSegments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ) * spotlightRadius;

            vertices[i + 1] = center + offset;

            // Build triangles
            if (i < spotlightSegments - 1)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            else
            {
                // close the circle
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = 1;
            }
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
