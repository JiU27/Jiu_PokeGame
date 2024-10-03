using UnityEngine;

public class OxygenStation : MonoBehaviour
{
    public float oxygenRefillRate = 1f; // ÿ�����ӵ�ʱ��
    public float maxGameTime = 300f; // �����Ϸʱ�䣨5���ӣ�

    public Material activeMaterial; // ����״̬�Ĳ���
    public Material inactiveMaterial; // �Ǽ���״̬�Ĳ���

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

        // ��ʼ������
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