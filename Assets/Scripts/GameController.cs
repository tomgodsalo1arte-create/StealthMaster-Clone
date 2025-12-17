using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [SerializeField] List<LevelScript> levelList = default;
    [SerializeField] GameObject upgradeLevel = default;
    [SerializeField] Image inputImage = default;
    [SerializeField] Image fadeImage = default;
    [SerializeField] PlayerBehaviour player = default;
    [SerializeField] GameObject itemSelectionTab = default;
    [SerializeField] MoneyMagnet moneyMagnet = default;
    [SerializeField] GameObject gameOverMenu = default;
    [SerializeField] TMP_Text moneyText = default;
    [SerializeField] HealthBarManager healthBar = default;
    int totalMoneyCount;
    int levelIndex = 0;
    LevelScript currentLevel;

    [Header("Music")]
    [SerializeField] private AudioSource backgroundMusic;  // normal BGM
    [SerializeField] private AudioSource alertMusic;       // tense/alert music
    [SerializeField] private float musicFadeTime = 0.5f;

    private bool inAlertState = false;
    private Coroutine musicFadeRoutine;

    [SerializeField] public List<EnemyBehaviour> enemies = new List<EnemyBehaviour>();

    private void Start()
    {
        Instance = this;
        GetLevel();

        // Make sure initial music state is correct
        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            if (!backgroundMusic.isPlaying)
                backgroundMusic.Play();
        }

        if (alertMusic != null)
        {
            alertMusic.loop = true;
            alertMusic.volume = 0f;  // start silent
            if (!alertMusic.isPlaying)
                alertMusic.Play();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ForceAllEnemysToBeDistracted();

    }
    // Called by enemies when they spot the player
    public void EnterAlertState()
    {
        SetAlertState(true);
    }

    // Called by enemies when they calm down
    public void ExitAlertState()
    {
        SetAlertState(false);
    }

    private void SetAlertState(bool alert)
    {
        if (inAlertState == alert)
            return;

        inAlertState = alert;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeMusic(alert));
    }
    public void ForceAllEnemiesToChase(Vector3 playerPosition)
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.ForceChase(playerPosition);
            }
        }
    }
    public void ForceAllEnemysToBeDistracted()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.ForceDistracted();
            }
        }
    }
    private IEnumerator FadeMusic(bool toAlert)
    {
        if (backgroundMusic == null || alertMusic == null)
            yield break;

        float fadeTime = musicFadeTime;

        float bgStart = backgroundMusic.volume;
        float alertStart = alertMusic.volume;

        float bgTarget = toAlert ? 0.3f : 1f;
        float alertTarget = toAlert ? 1f : 0f;

        // --- Fade In or Out ---
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float lerp = t / fadeTime;

            backgroundMusic.volume = Mathf.Lerp(bgStart, bgTarget, lerp);
            alertMusic.volume = Mathf.Lerp(alertStart, alertTarget, lerp);

            yield return null;
        }

        backgroundMusic.volume = bgTarget;
        alertMusic.volume = alertTarget;

        // --- If we just entered alert mode → stay there for 10 seconds ---
        if (toAlert)
        {
            yield return new WaitForSeconds(10f);

            // fade back after 10 sec
            StartCoroutine(FadeMusic(false));
        }
    }
    void SaveProgress()
    {

    }

    public void LevelCompleted()
    {
        inputImage.gameObject.SetActive(false);
        player.variableJoystick.Reset();

        player.transform.LookAt(levelList[levelIndex].lastPosition.position);
        LeanTween.move(player.gameObject, levelList[levelIndex].lastPosition.position, 1.5f).setOnComplete(() =>
        {
            player.transform.LookAt(Vector3.forward);
            LeanTween.alpha(fadeImage.rectTransform, 1, 1.5f).setOnComplete(() =>
            {
                player.transform.position = Vector3.zero;
                totalMoneyCount += moneyMagnet.moneyCount;
                moneyMagnet.moneyCount = 0;
                currentLevel.DisableAllEnemies();
                player.transform.LookAt(Vector3.forward);
                upgradeLevel.SetActive(true);
                LeanTween.alpha(fadeImage.rectTransform, 0, 1f).setOnComplete(() =>
                {
                    LeanTween.delayedCall(1f, () =>
                    {
                        fadeImage.raycastTarget = false;
                        itemSelectionTab.gameObject.SetActive(true);
                    });
                });        
                
            });
        });
    }

    public void UpgradeLevelCompleted()
    {
        fadeImage.raycastTarget = true;
        itemSelectionTab.SetActive(false);
        LeanTween.alpha(fadeImage.rectTransform, 1, 1f).setEase(LeanTweenType.easeInCirc).setOnComplete(() =>
        {
            upgradeLevel.SetActive(false);
            //if, liste bittiğinde hata vermemesi adına son bölümü tekrarlaması için.
            if (levelIndex < levelList.Count - 1)
            {
                levelIndex++;
            }
            GetLevel();
            LeanTween.alpha(fadeImage.rectTransform, 0, 1f).setEase(LeanTweenType.easeInCirc).setOnComplete(() => 
            {
                inputImage.gameObject.SetActive(true);
            });
        });
    }
    void GetLevel()
    {
        currentLevel = Instantiate(
            levelList[levelIndex],
            levelList[levelIndex].transform.position,
            Quaternion.identity
        );

        // Clear old enemies (important when restarting / next level)
        enemies.Clear();

        // Find all EnemyBehaviour components in the level
        EnemyBehaviour[] foundEnemies =
            currentLevel.GetComponentsInChildren<EnemyBehaviour>(true);

        enemies.AddRange(foundEnemies);

       // Debug.Log($"Enemies found in level: {enemies.Count}");
    }


    public void GameOver()
    {
        LeanTween.alpha(fadeImage.rectTransform, 0.5f, 1f).setOnComplete(() => 
        {
            gameOverMenu.SetActive(true);
            fadeImage.gameObject.SetActive(true);
            moneyText.text = moneyMagnet.moneyCount + "";
        });
    }

    public void Restart()
    {
        LeanTween.alpha(fadeImage.rectTransform, 1f, 1f).setOnComplete(() => 
        {
            player.gameObject.SetActive(true);
            healthBar.HealthBarChanged(100);
            moneyMagnet.moneyCount = 0;
            currentLevel.DisableAllEnemies();
            Destroy(currentLevel);
            GetLevel();
            gameOverMenu.SetActive(false);
            LeanTween.alpha(fadeImage.rectTransform, 0f, 1f);
        });
    }
}
