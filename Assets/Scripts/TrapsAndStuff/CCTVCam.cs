using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CCTVCam : MonoBehaviour
{
    /*[SerializeField]*/ private FieldOfView fieldOfView;
    public GameController controller;
    private bool isTurnedOff =false;
    private bool isSeen = false;
    private Vector3 playerPos;
    private RaycastHit hit;
    // [SerializeField] private LayerMask layerMask = default;

    [Header("Cam Rotation")]
    [SerializeField] private GameObject CamObject;
    [SerializeField] private float rotationAngle = 60f;   // total sweep angle
    [SerializeField] private float rotationSpeed = 30f;   // degrees per second
    [SerializeField] private float pauseTime = 0.5f;      // pause at ends

    [SerializeField] public CCTVSpotlightTrigger Spotlight;
    private float _targetY;
    private float _startX;
    private float _startY;
    private float _currentOffsetY;


    private float _pauseTimer;
    private int _direction = 1; // 1 = right, -1 = left
    private void OnEnable()
    {
        if (fieldOfView != null)
        {
            fieldOfView.OnTargetSeen += EnemysToAlert;
            fieldOfView.OnTargetLost += OnPlayerLost;
        }
    }
    private void OnDisable()
    {
        if (fieldOfView != null)
        {
            fieldOfView.OnTargetSeen -= EnemysToAlert;
            fieldOfView.OnTargetLost -= OnPlayerLost;
        }
    }
    void Start()
    {
        controller = GameController.Instance;

        Vector3 startEuler = CamObject.transform.localEulerAngles;

        _startX = startEuler.x;
        _startY = startEuler.y;

        // normalize X
        if (_startX > 180f) _startX -= 360f;

        _currentOffsetY = -rotationAngle * 0.5f;
    }



    private void Update()
    {
        if ((true))
        {
            RotateCam();
        }
      
    }

    public void RotateCam()
    {
        if (_pauseTimer > 0f)
        {
            _pauseTimer -= Time.deltaTime;
            return;
        }

        _currentOffsetY += rotationSpeed * Time.deltaTime * _direction;

        float halfAngle = rotationAngle * 0.5f;

        if (_currentOffsetY >= halfAngle)
        {
            _currentOffsetY = halfAngle;
            _direction = -1;
            _pauseTimer = pauseTime;
        }
        else if (_currentOffsetY <= -halfAngle)
        {
            _currentOffsetY = -halfAngle;
            _direction = 1;
            _pauseTimer = pauseTime;
        }

        CamObject.transform.localRotation =
            Quaternion.Euler(_startX, _startY + _currentOffsetY, 0f);
    }


    public void EnemysToAlert(Transform trans)
    {
       // Debug.Log("CCTV detected in EnemysToAlert CCTV class – alerting all enemies");
        //if (isTurnedOff) return;

        isSeen = true;

        if (controller != null)
        {
            controller.ForceAllEnemiesToChase(trans.position);
        }

        // Optional: make CCTV cone red
        fieldOfView.SetAlert(true);
    }
    //For CCTV cam--------------------------------------------------------------------------------------
    public void OnPlayerEnteredSpotlight(Transform player)
    {
        Debug.Log("Player entered CCTV spotlight");

        if (controller != null)
        {
            controller.ForceAllEnemiesToChase(player.position);
        }

        // Visual feedback
      //  fieldOfView.SetAlert(true);
    }

    public void OnPlayerExitedSpotlight()
    {
        Debug.Log("Player exited CCTV spotlight");

       // fieldOfView.SetAlert(false);
    }
    //End For CCTV cam--------------------------------------------------------------------------------------
    private void OnPlayerLost()
    {
        Debug.Log("Exiting field");
    }
    public void DeactivateCam()
    {
        // Read current Euler angles (degrees)
        Vector3 currentEuler = CamObject.transform.eulerAngles;

        // Force X to 90 degrees, keep Y and Z
        CamObject.transform.rotation = Quaternion.Euler(
            90f,
            currentEuler.y,
            currentEuler.z
        );
        Debug.Log("goona disable");
        // Optional: stop rotation logic completely
        enabled = false;

        if (Spotlight != null)
        {
            Spotlight.SpotlightToDeactiveColor();
        }
    }
 
}
