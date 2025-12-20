using UnityEngine;

public class PlayerFacing : MonoBehaviour
{
    [Header("Direction objects (children)")]
    public GameObject up;
    public GameObject down;
    public GameObject left;
    public GameObject right;

    [Header("Animators")]
    public Animator upAnim;
    public Animator downAnim;
    public Animator leftAnim;
    public Animator rightAnim;

    private Vector2 lastDir = Vector2.down;

    private void Reset() => AutoWire();
    private void Awake() => AutoWire();

    void Start()
    {
        Show(lastDir);
        SetAnim(false, lastDir);
        SnapToIdleSafe(lastDir); // başlangıç sprite'ını düzgün sabitle
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Bomberman: çapraz basınca baskın eksen
        if (Mathf.Abs(x) > Mathf.Abs(y)) y = 0;
        else x = 0;

        Vector2 dir = new Vector2(x, y);
        bool isMoving = dir != Vector2.zero;

        if (isMoving)
        {
            lastDir = dir;
            Show(lastDir);
        }

        SetAnim(isMoving, lastDir);

        if (!isMoving)
            SnapToIdleSafe(lastDir);
    }

    void Show(Vector2 dir)
    {
        bool faceUp = dir.y > 0.5f;
        bool faceDown = dir.y < -0.5f;
        bool faceLeft = dir.x < -0.5f;
        bool faceRight = dir.x > 0.5f;

        if (up) up.SetActive(faceUp);
        if (down) down.SetActive(faceDown);
        if (left) left.SetActive(faceLeft);
        if (right) right.SetActive(faceRight);
    }

    void SetAnim(bool moving, Vector2 dir)
    {
        bool faceUp = dir.y > 0.5f;
        bool faceDown = dir.y < -0.5f;
        bool faceLeft = dir.x < -0.5f;
        bool faceRight = dir.x > 0.5f;

        if (upAnim) upAnim.enabled = moving && faceUp;
        if (downAnim) downAnim.enabled = moving && faceDown;
        if (leftAnim) leftAnim.enabled = moving && faceLeft;
        if (rightAnim) rightAnim.enabled = moving && faceRight;
    }

    // ✅ HATASIZ: sadece aktif objede Animator.Update çağırır
    void SnapToIdleSafe(Vector2 dir)
    {
        Animator a = null;

        if (dir.y > 0.5f && up && up.activeInHierarchy) a = upAnim;
        else if (dir.y < -0.5f && down && down.activeInHierarchy) a = downAnim;
        else if (dir.x < -0.5f && left && left.activeInHierarchy) a = leftAnim;
        else if (dir.x > 0.5f && right && right.activeInHierarchy) a = rightAnim;

        if (a && a.gameObject.activeInHierarchy)
        {
            a.Rebind();
            a.Update(0f);
        }
    }

    void AutoWire()
    {
        Transform tUp = transform.Find("Up");
        Transform tDown = transform.Find("Down");
        Transform tLeft = transform.Find("Left");
        Transform tRight = transform.Find("Right");

        if (tUp) up = tUp.gameObject;
        if (tDown) down = tDown.gameObject;
        if (tLeft) left = tLeft.gameObject;
        if (tRight) right = tRight.gameObject;

        if (up) upAnim = up.GetComponent<Animator>();
        if (down) downAnim = down.GetComponent<Animator>();
        if (left) leftAnim = left.GetComponent<Animator>();
        if (right) rightAnim = right.GetComponent<Animator>();
    }
}
