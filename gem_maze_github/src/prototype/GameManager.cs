using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("遊戲設置")]
    public int currentLevel = 1;
    public float levelTimeLimit = 120f;
    public int targetGemCount = 10;
    
    [Header("UI 引用")]
    public Text scoreText;
    public Text timerText;
    public Text levelText;
    public Text gemCountText;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    
    [Header("音效")]
    public AudioClip backgroundMusic;
    public AudioClip gameOverSound;
    public AudioClip levelCompleteSound;
    
    // 遊戲狀態
    private int score = 0;
    private int collectedGems = 0;
    private float remainingTime;
    private bool isGamePaused = false;
    private bool isGameOver = false;
    private bool isLevelComplete = false;
    
    // 組件引用
    private AudioSource audioSource;
    private MazeGenerator mazeGenerator;
    
    // 單例實例
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // 實現單例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 獲取組件引用
        audioSource = GetComponent<AudioSource>();
        
        // 初始化UI
        InitializeUI();
    }
    
    void Start()
    {
        // 開始遊戲
        StartGame();
    }
    
    void Update()
    {
        // 如果遊戲暫停或結束，不更新
        if (isGamePaused || isGameOver || isLevelComplete)
            return;
            
        // 更新計時器
        UpdateTimer();
        
        // 檢查暫停輸入
        CheckPauseInput();
    }
    
    // 初始化UI
    void InitializeUI()
    {
        if (scoreText != null)
            scoreText.text = "分數: 0";
            
        if (timerText != null)
            timerText.text = "時間: " + levelTimeLimit.ToString("F0");
            
        if (levelText != null)
            levelText.text = "關卡: " + currentLevel;
            
        if (gemCountText != null)
            gemCountText.text = "寶石: 0/" + targetGemCount;
            
        // 隱藏面板
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
    
    // 開始遊戲
    public void StartGame()
    {
        // 重置遊戲狀態
        score = 0;
        collectedGems = 0;
        remainingTime = levelTimeLimit;
        isGamePaused = false;
        isGameOver = false;
        isLevelComplete = false;
        
        // 更新UI
        UpdateUI();
        
        // 播放背景音樂
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // 獲取迷宮生成器引用
        mazeGenerator = FindObjectOfType<MazeGenerator>();
        
        // 生成迷宮
        if (mazeGenerator != null)
        {
            // 根據關卡調整迷宮大小
            mazeGenerator.width = 10 + currentLevel;
            mazeGenerator.height = 10 + currentLevel;
            
            // 根據關卡調整寶石數量
            mazeGenerator.gemCount = targetGemCount;
            
            // 重新生成迷宮
            // 注意：在實際實現中，這裡需要先清除舊的迷宮
            // mazeGenerator.ClearMaze();
            // mazeGenerator.GenerateMaze();
        }
    }
    
    // 更新計時器
    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            GameOver();
        }
        
        if (timerText != null)
        {
            timerText.text = "時間: " + remainingTime.ToString("F0");
            
            // 時間不多時改變顏色
            if (remainingTime <= 30)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }
    }
    
    // 檢查暫停輸入
    void CheckPauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    // 切換暫停狀態
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            // 暫停遊戲
            Time.timeScale = 0;
            
            // 顯示暫停面板
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }
        else
        {
            // 恢復遊戲
            Time.timeScale = 1;
            
            // 隱藏暫停面板
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }
    
    // 收集寶石
    public void CollectGem(int value)
    {
        // 增加分數
        score += value;
        
        // 增加收集的寶石數量
        collectedGems++;
        
        // 更新UI
        UpdateUI();
        
        // 檢查是否收集了足夠的寶石
        if (collectedGems >= targetGemCount)
        {
            // 在這裡可以解鎖出口或提供其他獎勵
        }
    }
    
    // 完成關卡
    public void CompleteLevel()
    {
        if (isGameOver || isLevelComplete)
            return;
            
        isLevelComplete = true;
        
        // 計算時間獎勵
        int timeBonus = Mathf.FloorToInt(remainingTime);
        score += timeBonus;
        
        // 更新UI
        UpdateUI();
        
        // 顯示關卡完成面板
        if (levelCompletePanel != null)
        {
            Text bonusText = levelCompletePanel.GetComponentInChildren<Text>();
            if (bonusText != null)
                bonusText.text = "時間獎勵: " + timeBonus;
                
            levelCompletePanel.SetActive(true);
        }
        
        // 播放關卡完成音效
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(levelCompleteSound);
        }
        
        // 保存進度
        SaveProgress();
        
        // 延遲加載下一關
        StartCoroutine(LoadNextLevelAfterDelay(3f));
    }
    
    // 遊戲結束
    public void GameOver()
    {
        if (isGameOver || isLevelComplete)
            return;
            
        isGameOver = true;
        
        // 顯示遊戲結束面板
        if (gameOverPanel != null)
        {
            Text finalScoreText = gameOverPanel.GetComponentInChildren<Text>();
            if (finalScoreText != null)
                finalScoreText.text = "最終分數: " + score;
                
            gameOverPanel.SetActive(true);
        }
        
        // 播放遊戲結束音效
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // 保存分數
        SaveHighScore();
    }
    
    // 更新UI
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "分數: " + score;
            
        if (gemCountText != null)
            gemCountText.text = "寶石: " + collectedGems + "/" + targetGemCount;
    }
    
    // 保存高分
    void SaveHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }
    }
    
    // 保存進度
    void SaveProgress()
    {
        // 保存當前關卡
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        
        // 保存總分數
        int totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        PlayerPrefs.SetInt("TotalScore", totalScore + score);
        
        // 保存收集的寶石總數
        int totalGems = PlayerPrefs.GetInt("TotalGems", 0);
        PlayerPrefs.SetInt("TotalGems", totalGems + collectedGems);
        
        PlayerPrefs.Save();
    }
    
    // 延遲加載下一關
    IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 增加關卡
        currentLevel++;
        
        // 重新加載當前場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // 重新開始遊戲
    public void RestartGame()
    {
        // 恢復時間縮放
        Time.timeScale = 1;
        
        // 重新加載當前場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // 返回主菜單
    public void ReturnToMainMenu()
    {
        // 恢復時間縮放
        Time.timeScale = 1;
        
        // 加載主菜單場景
        SceneManager.LoadScene("MainMenu");
    }
}
