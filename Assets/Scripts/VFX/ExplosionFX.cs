using UnityEngine;

public class ExplosionFX : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.35f;

    private void OnEnable()
    {
        Invoke(nameof(Kill), lifeTime);
    }

    private void Kill()
    {
        Destroy(gameObject);
    }

    public void SetLifetime(float t)
    {
        lifeTime = t;
    }
}
