using UnityEngine;
using Unity.Netcode;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    void LateUpdate()
    {
        // Hedef yoksa sahibimiz olan Player'ý ara
        if (target == null)
        {
            foreach (var obj in Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                // SADECE bu bilgisayarýn sahibi olduðu (IsOwner) ve "Player" etiketli olaný bul
                if (obj.IsOwner && obj.CompareTag("Player"))
                {
                    target = obj.transform;
                    break;
                }
            }
        }

        // Hedef bulunduysa kamerayý oraya yumuþakça taþý
        if (target != null)
        {
            Vector3 targetPos = new Vector3(target.position.x, target.position.y, -10f);
            // Lerp ile kamera takibi daha akýcý olur
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
        }
    }
}