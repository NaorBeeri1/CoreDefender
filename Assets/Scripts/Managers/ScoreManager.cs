using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int scorePerEnemy = 100;
    [SerializeField] private TextMeshProUGUI scoreDisplayText;
    [SerializeField] private TextMeshProUGUI highScoreDisplayText;

    private int currentScore = 0;
    private int highScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighestScore", 0);
        UpdateUI();
    }

    private void OnEnable()
    {
        GameEventBus.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        GameEventBus.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void HandleEnemyDestroyed(int creditReward)
    {
        currentScore += scorePerEnemy;
        UpdateUI();
    }

    public void CheckAndSaveHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighestScore", highScore);
            PlayerPrefs.Save();
            Debug.Log($"[CoreDefender] New High Score achieved: {highScore}!");
        }
    }

    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => highScore;

    private void UpdateUI()
    {
        if (scoreDisplayText != null)
        {
            scoreDisplayText.text = $"Score: {currentScore}";
        }
        if (highScoreDisplayText != null)
        {
            highScoreDisplayText.text = $"High Score: {highScore}";
        }
    }
}