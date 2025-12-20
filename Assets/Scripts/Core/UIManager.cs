using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        if (!gameOverPanel)
            Debug.LogError("[UIManager] GameOverPanel not assigned!");
    }

    public void ShowGameOver()
    {
        Debug.Log("[UIManager] ShowGameOver()");
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // oyunu durdur
    }
}
