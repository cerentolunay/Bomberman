using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 4f;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;

    private Vector2 input;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!anim) anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Bomberman hissi: çapraz basýnca baskýn ekseni seç
        if (Mathf.Abs(x) > Mathf.Abs(y)) y = 0;
        else x = 0;

        input = new Vector2(x, y).normalized;

        // Sað/Sol dönme (flip)
        if (input.x > 0.01f) sr.flipX = false;
        else if (input.x < -0.01f) sr.flipX = true;

        // Animator: sadece Idle/Walk
        if (anim)
        {
            anim.SetBool("IsMoving", input != Vector2.zero);
        }
    }

    private void FixedUpdate()
    {
        if (!rb) return;
        rb.linearVelocity = input * speed;
    }
}
