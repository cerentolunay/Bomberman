using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        if (!gameOverPanel)
            Debug.LogError("[UIManager] GameOverPanel not assigned!");

        if (!pausePanel)
            Debug.LogWarning("[UIManager] PausePanel not assigned!");

        Debug.Log($"[UIManager Awake] I am: {name} | instanceID={GetInstanceID()}");
        Debug.Log($"[UIManager Awake] gameOverPanel={(gameOverPanel ? gameOverPanel.name : "NULL")}, pausePanel={(pausePanel ? pausePanel.name : "NULL")}");

        if (pausePanel) pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void ShowPause()
    {
        if (!pausePanel) return;
        pausePanel.SetActive(true);
        pausePanel.transform.SetAsLastSibling();
        Time.timeScale = 0f;
    }

    public void HidePause()
    {
        Debug.Log($"[UIManager] HidePause() -> disabling: {(pausePanel ? pausePanel.name : "NULL")} | this={name} id={GetInstanceID()}");
        if (pausePanel) pausePanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        Debug.Log($"[UIManager] ShowGameOver() -> enabling: {(gameOverPanel ? gameOverPanel.name : "NULL")} | this={name} id={GetInstanceID()}");
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideGameOver()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnResumeClicked()
    {
        GameManager.Instance.ResumeFromPause();
    }

    public void OnMainMenuClicked()
    {
        GameManager.Instance.GoToMainMenu();
    }

}
