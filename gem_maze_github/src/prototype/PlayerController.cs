using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設置")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("收集設置")]
    public float collectDistance = 1.5f;
    public ParticleSystem collectEffect;
    
    [Header("音效")]
    public AudioClip moveSound;
    public AudioClip collectSound;
    public AudioClip exitSound;
    
    // 組件引用
    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;
    
    // 移動輸入
    private Vector3 moveDirection;
    private bool isMoving = false;
    
    // 遊戲管理器引用
    private GameManager gameManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
        
        // 確保剛體不會旋轉
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    
    void Update()
    {
        // 處理移動輸入
        HandleInput();
        
        // 檢查寶石收集
        CheckGemCollection();
        
        // 檢查是否到達出口
        CheckExit();
        
        // 更新動畫
        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        // 移動角色
        MovePlayer();
    }
    
    // 處理輸入
    void HandleInput()
    {
        // 獲取水平和垂直輸入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 移動方向
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // 判斷是否移動
        isMoving = moveDirection.magnitude > 0.1f;
        
        // 觸摸/滑動控制（適用於移動設備）
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Moved)
            {
                // 將觸摸移動轉換為移動方向
                Vector2 touchDelta = touch.deltaPosition;
                moveDirection = new Vector3(touchDelta.x, 0f, touchDelta.y).normalized;
                isMoving = true;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                moveDirection = Vector3.zero;
                isMoving = false;
            }
        }
    }
    
    // 移動角色
    void MovePlayer()
    {
        if (isMoving)
        {
            // 使用剛體移動
            rb.velocity = moveDirection * moveSpeed;
            
            // 旋轉角色面向移動方向
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // 播放移動音效
            if (!audioSource.isPlaying && moveSound != null)
            {
                audioSource.clip = moveSound;
                audioSource.Play();
            }
        }
        else
        {
            // 停止移動
            rb.velocity = Vector3.zero;
            
            // 停止音效
            if (audioSource.clip == moveSound)
            {
                audioSource.Stop();
            }
        }
    }
    
    // 檢查寶石收集
    void CheckGemCollection()
    {
        // 獲取所有寶石
        Gem[] gems = FindObjectsOfType<Gem>();
        
        foreach (Gem gem in gems)
        {
            // 計算與寶石的距離
            float distance = Vector3.Distance(transform.position, gem.transform.position);
            
            // 如果距離足夠近，收集寶石
            if (distance < collectDistance)
            {
                // 播放收集效果
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, gem.transform.position, Quaternion.identity);
                }
                
                // 播放收集音效
                if (collectSound != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }
                
                // 通知遊戲管理器
                gameManager.CollectGem(gem.gemValue);
                
                // 銷毀寶石
                Destroy(gem.gameObject);
            }
        }
    }
    
    // 檢查是否到達出口
    void CheckExit()
    {
        // 獲取出口位置
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");
        
        if (exit != null)
        {
            // 計算與出口的距離
            float distance = Vector3.Distance(transform.position, exit.transform.position);
            
            // 如果距離足夠近，完成關卡
            if (distance < collectDistance)
            {
                // 播放出口音效
                if (exitSound != null)
                {
                    audioSource.PlayOneShot(exitSound);
                }
                
                // 通知遊戲管理器
                gameManager.CompleteLevel();
            }
        }
    }
    
    // 更新動畫
    void UpdateAnimation()
    {
        if (animator != null)
        {
            // 設置移動動畫參數
            animator.SetBool("IsMoving", isMoving);
            
            // 設置移動速度參數（用於混合動畫）
            animator.SetFloat("MoveSpeed", rb.velocity.magnitude / moveSpeed);
        }
    }
    
    // 升級移動速度
    public void UpgradeSpeed(float speedIncrease)
    {
        moveSpeed += speedIncrease;
    }
    
    // 升級收集範圍
    public void UpgradeCollectRange(float rangeIncrease)
    {
        collectDistance += rangeIncrease;
    }
}
