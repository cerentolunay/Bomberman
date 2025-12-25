using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTestUI : MonoBehaviour
{
    public Button hostBtn;
    public Button clientBtn;
    public string targetLevelName = "Level_Desert"; // Buraya hangi haritayý istiyorsan onu yaz

    void Start()
    {
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();

            // Host baþladýktan sonra sahneyi yükle
            // Bu komut sayesinde baðlanan herkes otomatik olarak bu sahneye geçer
            NetworkManager.Singleton.SceneManager.LoadScene(targetLevelName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        });

        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
}