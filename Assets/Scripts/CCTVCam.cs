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
    [SerializeField] private float rotationAngle = 60f;   // total sweep angle
    [SerializeField] private float rotationSpeed = 30f;   // degrees per second
    [SerializeField] private float pauseTime = 0.5f;      // pause at ends

    private float _startY;
    private float _targetY;
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

        //for cam rotation
        _startY = transform.eulerAngles.y;
        _targetY = _startY + rotationAngle * 0.5f;
    }

    private void Update()
    {

        //if (isTurnedOff)
        //    return;

      /*  FieldOfViewHandle();*/

        RotateCam();
    }
    /*private void FieldOfViewHandle()
    {
        //Debug.Log("CCTV scene");
        fieldOfView.SetAimDirection(transform.forward);
        fieldOfView.SetOrigin(transform.position);
        fieldOfView.SetAimDirection(transform.forward);


    }*/
    public void RotateCam()
    {
         if (_pauseTimer > 0f)
        {
            _pauseTimer -= Time.deltaTime;
            return;
        }

        float currentY = transform.eulerAngles.y;
        float step = rotationSpeed * Time.deltaTime * _direction;

        float nextY = currentY + step;

        // Check if we reached or passed the target
        if ((_direction > 0 && nextY >= _targetY) ||
            (_direction < 0 && nextY <= _targetY))
        {
            nextY = _targetY;
            _direction *= -1;

            _targetY = _startY + (_direction * rotationAngle * 0.5f);
            _pauseTimer = pauseTime;
        }

        transform.rotation = Quaternion.Euler(25f, nextY, 0f);
    
    }
    public void EnemysToAlert(Transform trans)
    {
       // Debug.Log("CCTV detected in EnemysToAlert CCTV class – alerting all enemies");
        //if (isTurnedOff) return;

        isSeen = true;

        if (GameController.Instance != null)
        {
            GameController.Instance.ForceAllEnemiesToChase(trans.position);
        }

        // Optional: make CCTV cone red
        fieldOfView.SetAlert(true);
    }
    //For CCTV cam--------------------------------------------------------------------------------------
    public void OnPlayerEnteredSpotlight(Transform player)
    {
        Debug.Log("Player entered CCTV spotlight");

        if (GameController.Instance != null)
        {
            GameController.Instance.ForceAllEnemiesToChase(player.position);
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

}
