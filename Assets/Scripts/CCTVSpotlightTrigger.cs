using UnityEngine;

public class CCTVSpotlightTrigger : MonoBehaviour
{
    [SerializeField] private CCTVCam cctv;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = normalColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Turn spotlight RED
        spriteRenderer.color = alertColor;

        // Notify CCTV
        cctv.OnPlayerEnteredSpotlight(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Return spotlight to NORMAL
        spriteRenderer.color = normalColor;

        // Notify CCTV
        cctv.OnPlayerExitedSpotlight();
    }
}
