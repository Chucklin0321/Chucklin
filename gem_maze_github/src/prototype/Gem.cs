using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("寶石設置")]
    public int gemValue = 1;
    public GemType gemType = GemType.Common;
    public float rotationSpeed = 50f;
    public float bobSpeed = 1f;
    public float bobHeight = 0.2f;
    
    [Header("視覺效果")]
    public Material gemMaterial;
    public Color gemColor = Color.blue;
    public Light gemLight;
    
    // 寶石類型枚舉
    public enum GemType
    {
        Common,     // 普通寶石
        Rare,       // 稀有寶石
        Epic,       // 史詩寶石
        Legendary   // 傳說寶石
    }
    
    // 初始位置
    private Vector3 startPosition;
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        // 記錄初始位置
        startPosition = transform.position;
        
        // 獲取網格渲染器
        meshRenderer = GetComponent<MeshRenderer>();
        
        // 設置寶石材質和顏色
        SetupGemAppearance();
        
        // 根據寶石類型設置價值
        SetGemValueByType();
    }
    
    void Update()
    {
        // 旋轉寶石
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // 上下浮動
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    // 設置寶石外觀
    void SetupGemAppearance()
    {
        if (meshRenderer != null && gemMaterial != null)
        {
            // 創建材質實例
            Material instanceMaterial = new Material(gemMaterial);
            
            // 根據寶石類型設置顏色
            switch (gemType)
            {
                case GemType.Common:
                    gemColor = Color.blue;
                    break;
                case GemType.Rare:
                    gemColor = Color.green;
                    break;
                case GemType.Epic:
                    gemColor = Color.magenta;
                    break;
                case GemType.Legendary:
                    gemColor = Color.yellow;
                    break;
            }
            
            // 設置材質顏色
            instanceMaterial.color = gemColor;
            
            // 應用材質
            meshRenderer.material = instanceMaterial;
        }
        
        // 設置光源顏色和強度
        if (gemLight != null)
        {
            gemLight.color = gemColor;
            
            // 根據寶石類型設置光源強度
            switch (gemType)
            {
                case GemType.Common:
                    gemLight.intensity = 0.5f;
                    break;
                case GemType.Rare:
                    gemLight.intensity = 1.0f;
                    break;
                case GemType.Epic:
                    gemLight.intensity = 1.5f;
                    break;
                case GemType.Legendary:
                    gemLight.intensity = 2.0f;
                    break;
            }
        }
    }
    
    // 根據寶石類型設置價值
    void SetGemValueByType()
    {
        switch (gemType)
        {
            case GemType.Common:
                gemValue = 1;
                break;
            case GemType.Rare:
                gemValue = 5;
                break;
            case GemType.Epic:
                gemValue = 20;
                break;
            case GemType.Legendary:
                gemValue = 100;
                break;
        }
    }
    
    // 當被收集時調用
    public void Collect()
    {
        // 播放收集動畫
        PlayCollectAnimation();
        
        // 延遲銷毀，讓動畫有時間播放
        Destroy(gameObject, 0.5f);
    }
    
    // 收集動畫
    void PlayCollectAnimation()
    {
        // 縮放效果
        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);
        
        // 上升效果
        LeanTween.moveY(gameObject, transform.position.y + 1f, 0.5f).setEase(LeanTweenType.easeOutQuad);
    }
}
