using UnityEngine;

public class OxygenStation : MonoBehaviour
{
    public float oxygenRefillRate = 1f; // 每秒增加的时间
    public float maxGameTime = 300f; // 最大游戏时间（5分钟）

    public Material activeMaterial; // 激活状态的材质
    public Material inactiveMaterial; // 非激活状态的材质

    private bool isActive = false;
    private GameControl gameControl;
    private Renderer stationRenderer;

    private void Start()
    {
        gameControl = FindObjectOfType<GameControl>();
        if (gameControl == null)
        {
            Debug.LogError("GameControl not found in the scene!");
        }

        stationRenderer = GetComponent<Renderer>();
        if (stationRenderer == null)
        {
            Debug.LogError("Renderer component not found on OxygenStation!");
        }

        // 初始化材质
        UpdateMaterial();
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.CompareTag("Player") && gameControl != null)
        {
            gameControl.AddTime(oxygenRefillRate * Time.deltaTime);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        if (stationRenderer != null)
        {
            stationRenderer.material = isActive ? activeMaterial : inactiveMaterial;
        }
    }
}