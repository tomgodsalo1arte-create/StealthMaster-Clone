using EZCameraShake;
using System;
using System.Collections;
using UnityEngine;

public class PlayerBehaviour : CharacterBaseScript
{
    public static PlayerBehaviour Instance { get; private set; }
    public FloatingJoystick variableJoystick;
    [SerializeField] private LayerMask layerMask = default;
    [SerializeField] private GameObject shuriken = default;
    [SerializeField] private HealthBarManager healthBar = default;
    [SerializeField] private float turnSmoothTime = 0.125f;
    [SerializeField] private Animation anim ;
    [SerializeField] private Animator animtor = null;
    public float throwSpeed;
    [HideInInspector] public Vector3 direction;
    public bool hasKey = false;
    private bool died = false;
    public static event Action OpenTheDoor;
    private Vector3 enemyPos;
    private float turnSmoothVelocity;
    private RaycastHit hit;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip gotHitClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip footstepsClip;
    private bool canPlayStep = true;
    private float stepInterval = 0.3f;

    private void Awake()
    {
        Instance = this;
        EnemyProjectile.GotHit += GotShot;
        healthBar.HealthReachedZero += Died;
        rigidBody = GetComponent<Rigidbody>();
        healthBar.IsHealthFull();
        variableJoystick.Reset();
       anim.Play("AttackIdle");
        animtor.Play("Idle");
    }

    private void FixedUpdate()
    {
      
        dummyBody.transform.localPosition = Vector3.zero;//animation problem, stop the minute drifting
        if (!died)
        {
            HandleMovement();
        }
        if (Input.GetKeyDown(KeyCode.Q)) animtor.Play("Death");
    }

    private void HandleMovement()
    {
        direction = Vector3.forward * variableJoystick.Vertical + Vector3.right * variableJoystick.Horizontal;

        if (direction.magnitude >= 0.1f)
        {
            anim.Play("Run");
            animtor.Play("Fast Run");

            if (canPlayStep)
                StartCoroutine(PlayAudio());

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            rigidBody.linearVelocity = direction * speed;
        }

        else
        {
            rigidBody.linearVelocity = Vector3.zero;
            anim.Play("AttackIdle");
            animtor.Play("Idle");
        }
    }

    private IEnumerator PlayAudio()
    {
        canPlayStep = false;

        if (audioSource != null && footstepsClip != null)
            audioSource.PlayOneShot(footstepsClip);

        yield return new WaitForSeconds(stepInterval);

        canPlayStep = true;
    }
    private void Attack()
    {
        if (Physics.Raycast(transform.position + Vector3.up, enemyPos - transform.position, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                //  CameraShaker.Instance.ShakeOnce(0.5f, 4f, 0f, 0.08f);
                CameraShaker.Instance.ShakeOnce(3.5f, 9f, 0f, 0.18f);
                animtor.Play("Standing Melee Attack Downward");
                anim.Play("D_Attack");

                if (audioSource != null && gotHitClip != null)
                {
                    audioSource.PlayOneShot(gotHitClip);
                }
                if (audioSource != null && throwClip != null)
                {
                    audioSource.PlayOneShot(throwClip);
                }
                Projectile spawnedShuriken = Instantiate(shuriken, new Vector3(transform.position.x, 1f, transform.position.z), Quaternion.identity).GetComponent<Projectile>();
                spawnedShuriken.targetEnemy = hit.transform;
                spawnedShuriken.speed = throwSpeed;
            }
        }
    }

    private void GotShot()
    {
        healthBar.HealthBarChanged(-5);
        CameraShaker.Instance.ShakeOnce(4.5f, 14f, 0f, 0.25f);

        if (audioSource != null && gotHitClip != null)
            audioSource.PlayOneShot(gotHitClip);

    }

    private void Died()
    {
        died = true;
        animtor.Play("Death");

        if (audioSource != null && deathClip != null)
            audioSource.PlayOneShot(deathClip);

        transform.tag = "Dead";
        GameController.Instance.GameOver();
    }

    void TurnToItem()
    {
        //Turn to an item when not moving. Requires upgrade.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (died)
        {
            return;
        }
        if (other.CompareTag("Enemy"))
        {
            enemyPos = other.transform.position;
            Attack();
        }

        if (other.CompareTag("Door"))
        {
            if (hasKey)
            {
                OpenTheDoor?.Invoke();
            }
        }
    }
}
