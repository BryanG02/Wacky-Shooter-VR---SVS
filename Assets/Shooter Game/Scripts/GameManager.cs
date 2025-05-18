using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button retryButton;

    [Header("Game References")]
    [SerializeField] private Player player;
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Game Stats")]
    [SerializeField] private TMP_Text timeSurvivedText;
    [SerializeField] private TMP_Text enemiesDefeatedText;

    [Header("High Score")]
    [SerializeField] private TMP_Text highScoreText;
    private int _highScore;

    [Header("Exit Buttons")]
    [SerializeField] private Button quitButton1;
    [SerializeField] private Button quitButton2;


    private float _gameTime;
    private int _enemiesKilled;

    private void Update()
    {
        if (player.gameObject.activeSelf)
        {
            _gameTime += Time.deltaTime;
        }
    }

    private void Awake()
    {
        startButton.onClick.AddListener(StartNewGame);
        retryButton.onClick.AddListener(StartNewGame);
        LoadHighScore();

        // Add quit button listeners
        if(quitButton1 != null) quitButton1.onClick.AddListener(QuitGame);
        if(quitButton2 != null) quitButton2.onClick.AddListener(QuitGame);
        
        InitializeGameState();
    }

    private void InitializeGameState()
    {
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        enemySpawner.SetSpawningActive(false);
    }

    public void StartNewGame()
    {
        // Clean up existing enemies
        enemySpawner.ResetSpawner();

        // Reset stats
        _gameTime = 0f;
        _enemiesKilled = 0;
        UpdateGameOverStats();
        
        // Reset player
        player.ResetPlayer();
        
        // Activate systems
        enemySpawner.SetSpawningActive(true);
        
        // Update UI
        startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void GameOver()
    {
        UpdateGameOverStats();
        enemySpawner.ResetSpawner();
        enemySpawner.SetSpawningActive(false);
        gameOverPanel.SetActive(true);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void RegisterEnemyKill()
    {
        _enemiesKilled++;
        CheckHighScore();
    }

    private void CheckHighScore()
    {
        if (_enemiesKilled > _highScore)
        {
            _highScore = _enemiesKilled;
            PlayerPrefs.SetInt("HighScore", _highScore);
            
        }
    }

    private void UpdateGameOverStats()
    {
        bool newHighScore = false;

        // Update UI
        if (timeSurvivedText)
            timeSurvivedText.text = $"Time Survived: {FormatTime(_gameTime)}";
        
        if (enemiesDefeatedText)
            enemiesDefeatedText.text = $"Enemies Defeated: {_enemiesKilled}";
        
        if (highScoreText)
            highScoreText.text = $"High Score: {_highScore}" + 
                                (newHighScore ? " (NEW!)" : "");
    }

    private void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }
}