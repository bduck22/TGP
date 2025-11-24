using UnityEngine;

public class Monster : MonoBehaviour
{
    public int Hp;
    public float Speed;
    public float JumpForce;
    public float HeightThreshold = 1.5f;

    public Transform Player;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Hp = 1;
    }

    void Update()
    {
        if (Player.gameObject.activeSelf)
        {
            MoveTowardsPlayer();
            CheckAndJump();
        }
    }

    void MoveTowardsPlayer()
    {
        if (Player == null || rb == null) return;

        Vector2 direction = new Vector2(Player.position.x - transform.position.x, 0);

        float horizontalVelocity = direction.normalized.x * Speed;

        rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y);

        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void CheckAndJump()
    {
        if (Player == null || rb == null) return;

        float heightDifference = Player.position.y - transform.position.y;

        if (heightDifference >= HeightThreshold && isGrounded)
        {
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            Vector2 normal = collision.GetContact(0).normal;
            if (normal.y > 0.9f)
            {
                isGrounded = true;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {

    }
}