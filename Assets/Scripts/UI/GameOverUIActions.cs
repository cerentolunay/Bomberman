using UnityEngine;

public class GameOverUIActions : MonoBehaviour
{
    public void Restart()
    {
        GameManager.Instance.RestartLevel();
    }

    public void MainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
