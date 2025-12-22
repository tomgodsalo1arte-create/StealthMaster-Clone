using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : CharacterBaseScript
{
    [SerializeField] private FieldOfView fieldOfView;
    [Header("DetectionField")]
    [SerializeField] private float visionConeAngle = default;
    [SerializeField] private float visionRange = default;
    [SerializeField] private LayerMask layerMask = default;

    [Header("Waypoint")]
    [SerializeField] private GameObject waypoints = default;
    private List<GameObject> waypointList;
    private int waypointIndex = 0;

    private bool patrolling;
    private Vector3 latestPosition;
    private Vector3 playerPos;

    [SerializeField] private float cooldownTime = default;
    private bool _isCooldown = false;
    [SerializeField] private GameObject shuriken = default;
    [SerializeField] private float throwSpeed = default;

    public Vector3 targetPos;

    private RaycastHit hit;

    private bool isDead = false;
    private bool isSeen = false;

    [SerializeField] private Animation anim;
    [SerializeField] private Animator animtor = null;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    public AudioSource AudioSource => audioSource;
    [SerializeField] private AudioClip spottedClip;
    [SerializeField] private AudioClip gotHitClip;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip deathClip;
    private bool isInAlert = false;

    private NavMeshAgent agent ;
    private StateMachine _stateMachine ;

    // [SerializeField] private float distractedCooldown = 120f; // 2 minutes
     private float _lastDistractedTime = -Mathf.Infinity;

    // We'll keep references to states so we don't keep "new"ing them.
    private EnemyPatrolState _patrolState;
    private EnemyAttackState _attackState;
    private EnemyAlertState _alertState;
    private EnemyChaseState _chaseState;
    private EnemyDeadState  _deadState;
    private EnemyDistractedState  _distractedState;
    private EnemyIdealStaticState _idealState;
    public int WaypointCount => waypointList?.Count ?? 0;
   
    public Vector3 GetWaypointPosition(int index)
    {
        if (waypointList == null || index < 0 || index >= waypointList.Count)
            return transform.position;
        return waypointList[index].transform.position;
    }

    // Optionally expose patrol speed, anims, and death flag:
    public float PatrolSpeed = 1.5f;  // tune this in inspector if you like

    [Header("Alert / Hunt Settings")]
    [SerializeField] private float alertStandTime = 1f;   // stand still after losing sight
    [SerializeField] private float maxHuntTime = 10f;     // how long to hunt
    [SerializeField] private float alertWalkSpeed = 2f;   // “alert walk” speed

    private Vector3 originalPosition;

    public bool IsDead { get => isDead; set => isDead = value; }
    public Vector3 OriginalPosition => originalPosition;
    public bool IsInAlert
    {
        get => isInAlert;
        set => isInAlert = value;
    }
    public FieldOfView FieldOfView => fieldOfView;

    public Vector3 LastKnownPlayerPosition { get; set; }

    public float AlertStandTime => alertStandTime;
    public float MaxHuntTime => maxHuntTime;
    public float AlertWalkSpeed => alertWalkSpeed;
    public Animator Animator => animtor;

    private int _currentWaypointIndex = -1;
    public Vector3 CurrentWaypointTarget { get; private set; }

    private void OnEnable()
    {
        if (fieldOfView != null)
        {
            fieldOfView.OnTargetSeen += PlayerSeen;
            fieldOfView.OnTargetLost += OnPlayerLost;
        }
    }
    private void OnDisable()
    {
        if (fieldOfView != null)
        {
            fieldOfView.OnTargetSeen -= PlayerSeen;
            fieldOfView.OnTargetLost -= OnPlayerLost;
        }
    }
    private void Awake()
    {

        agent  = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();

        _stateMachine = new StateMachine();
        // state Instences
      
        //Adding Waypoints to the list
        waypointList = new List<GameObject>();
        if (waypoints.transform.childCount > 0)
        {
            patrolling = true;
            for (int i = 0; i < waypoints.transform.childCount; i++)
            {
                waypointList.Add(waypoints.transform.GetChild(i).gameObject);
            }
        }
        _patrolState = new EnemyPatrolState(this, _stateMachine, agent);
        _chaseState = new EnemyChaseState(this, _stateMachine, agent);
        _attackState = new EnemyAttackState(this, _stateMachine, agent);
        _deadState = new EnemyDeadState(this, _stateMachine, agent);
        _alertState = new EnemyAlertState(this, _stateMachine, agent);
        _distractedState = new EnemyDistractedState(this, _stateMachine, agent);
        _idealState = new EnemyIdealStaticState(this, _stateMachine, agent);
        //anim.Play("AttackIdle");
        //animtor.Play("Idle");
        // StartCoroutine(EnemyAI()); // set in patrolState ----------------------------------------------------  Done
    }
    private void Start()
    {
        _stateMachine.Initialize(_patrolState);
    }
    private void Update()
    {
        _stateMachine.Update();

        if (isDead)
            return;

        FieldOfViewHandle();
    }
    private void FoundDeadEnemy(Transform transform)
    {
        Debug.Log("(hit.collider.CompareTag(\"Dead\")--Inside FoundDeadEnemy");
        SwitchToPatrolFromAlert();
    }
    public IEnumerator TurnTowards(Vector3 direction, float turnSpeed)
    {
        _isTurning = true;
        agent.updateRotation = false;

        Quaternion target = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, target) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                turnSpeed * Time.deltaTime
            );
            yield return null;
        }

        agent.updateRotation = true;
        _isTurning = false;
    }
    public void SetNextWaypointDestination(NavMeshAgent agent)
    {
      //  Debug.Log("EnemyBehaviour → SetNextWaypointDestination");

        if (WaypointCount == 0) return;

        int nextIndex = _currentWaypointIndex;

        if (WaypointCount == 1)
        {
            nextIndex = 0;
        }
        else
        {
            while (nextIndex == _currentWaypointIndex)
            {
                // nextIndex = UnityEngine.Random.Range(0, WaypointCount);
                nextIndex = _currentWaypointIndex++;
            }
        }
        _currentWaypointIndex = nextIndex;

        CurrentWaypointTarget = GetWaypointPosition(_currentWaypointIndex);

        agent.isStopped = false;
        agent.SetDestination(CurrentWaypointTarget);

        Animator.Play("Walking");
    }
    [Header("LOOKING AROUND ")]
    [SerializeField] public float scanInterval = 0.6f;
    public float _scanTimer;
    private bool _isScanning;
    private float _baseYaxis;
    private bool _lookInitialized;
    private bool _lookingRight;
    [SerializeField] private float lookAngle = 90f;   // or 180f
    [SerializeField] private float turnSpeed = 180f;  // degrees/sec
    public bool _waitingAtWaypoint;
    private bool _isTurning;

    public void LookSideToSide()
    {
        if (_isTurning) return;

        if (!_lookInitialized)
        {
            _baseYaxis = transform.eulerAngles.y;
            _lookingRight = false;
            _lookInitialized = true;
        }

        float targetYaw = _lookingRight
            ? _baseYaxis + lookAngle
            : _baseYaxis - lookAngle;

        Vector3 lookDir = Quaternion.Euler(0, targetYaw, 0) * Vector3.forward;

        StartCoroutine(TurnTowards(lookDir, turnSpeed));

        _lookingRight = !_lookingRight;
    }
    /*public void ResetLook()
    {
        _lookInitialized = false;
    }*/
    private void FieldOfViewHandle()
    {
        fieldOfView.SetAimDirection(transform.forward);
        fieldOfView.SetOrigin(transform.position + new Vector3(0, 0.1f, 0));
    }
    public void SwitchToPatrolFromAlert()
    {
        if (isInAlert && GameController.Instance != null)
        {
            GameController.Instance.ExitAlertState();
            isInAlert = false;

            fieldOfView.SetAlert(false); // switch cone to white
        }

        _stateMachine.TransitionTo(_patrolState);
    }

    public void PlayerSeen(Transform trans)
    {   
        isSeen = true;
        
        // Store last known position for alert/hunt logic
        LastKnownPlayerPosition = trans.position;

        // Go into Attack state – it will handle LookAt, Attack, and alert visuals
       _stateMachine.TransitionTo(_attackState);
        
    }
    private void OnPlayerLost()
    {
        if (isDead) return;
        isSeen = false;
        _stateMachine.TransitionTo(_chaseState);
    }
   
    public void Died()
    {
        if (isDead) return;

        isDead = true;
        _stateMachine.TransitionTo(_deadState);
    }
    public void ForceChase(Vector3 playerPosition)
    {
        if (IsDead) return;

        LastKnownPlayerPosition = playerPosition;

        // Enter global alert once
        if (!IsInAlert && GameController.Instance != null)
        {
            GameController.Instance.EnterAlertState();
            IsInAlert = true;
            FieldOfView.SetAlert(true);
        }

        _stateMachine.TransitionTo(_alertState); 
        //  _stateMachine.TransitionTo(_chaseState);
      
    }
    public void Attack()
    {
        if (!_isCooldown)
        {
            animtor.Play("Attack");

            if (audioSource != null && throwClip != null)
                audioSource.PlayOneShot(throwClip);
            if (audioSource != null && gotHitClip != null)
                audioSource.PlayOneShot(gotHitClip);

            EnemyProjectile spawnedShuriken = Instantiate(shuriken, new Vector3(transform.position.x, 1f, transform.position.z), Quaternion.identity).GetComponent<EnemyProjectile>();
            spawnedShuriken.player = PlayerBehaviour.Instance.transform;
            spawnedShuriken.speed = throwSpeed;
            StartCoroutine(AttackCooldown());
        }
    }
    public IEnumerator AttackCooldown()
    {
        _isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        _isCooldown = false;
    }

    internal void ForceDistracted()
    {
        if (CanEnterDistractedState())
        {
            Debug.Log("PowerUp started");
            MarkDistractedUsed();
            _stateMachine.TransitionTo(_distractedState);
        }

    }
    public bool CanEnterDistractedState()
    {
        if (GameController.Instance == null)
        {
            Debug.Log("Controllert is nuull on enemy Obj");
            return false;
        }
        return Time.time >= _lastDistractedTime + GameController.Instance.DistractedCooldown;
    }
    public void MarkDistractedUsed()
    {
        _lastDistractedTime = Time.time;
    }
}
