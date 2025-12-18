using UnityEngine;

public class CCTVSpotlightTrigger : MonoBehaviour
{
    [SerializeField] private CCTVCam cctv;
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color grColor = Color.gray;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = normalColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") )
            return;

      

        // Notify CCTV
        cctv.OnPlayerEnteredSpotlight(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Return spotlight to NORMAL
        SpotlightToNormal();

        // Notify CCTV
        cctv.OnPlayerExitedSpotlight();
    }
    public void SpotlightToRed()
    {
        // Turn spotlight RED
        spriteRenderer.color = alertColor;
    }
    public void SpotlightToDeactiveColor()
    {
        // Turn spotlight RED
        spriteRenderer.color = grColor;
    }
    public void SpotlightToNormal()
    {
        // Return spotlight to NORMAL
        spriteRenderer.color = normalColor;
    }
}
