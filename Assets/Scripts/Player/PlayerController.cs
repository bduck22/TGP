using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed;
    Rigidbody2D rigidbody2D;
    public bool isGround;
    public int jumpCount;
    public float jumpPower;

    public int bulletCount;

    [SerializeField] private Bullet Bullet;
    [SerializeField] SpriteRenderer Renderer;
    [SerializeField] private Sprite BoxSprite;
    [SerializeField] private Sprite CircleSprite;

    public float Invin;
    public bool Parring;
    public float time;
    public float dashtime;
    public float dashcool;
    public float cool;
    public Vector2 MouseP;
    public bool dashing;
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public bool stone;
    void Update()
    {

        if (stone)
        {
            if (!Cursor.visible)
            {
                Cursor.visible = true;
                BulletCount.gameObject.SetActive(false);
            }
            return;
        }
        else
        {
            if (Cursor.visible)
            {
                Cursor.visible = false;
                BulletCount.gameObject.SetActive(true);
            }
            MouseP = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            BulletCount.position = MouseP;
        }
        if (Invin > 0)
        {
            Invin -= Time.deltaTime;
        }
        else Invin = 0;
        if (time < cool)
        {
            time += Time.deltaTime;
        }
        if (dashtime < dashcool)
        {
            dashtime += Time.deltaTime;
        }

        transform.LookAt(MouseP);

        if (Input.GetMouseButtonDown(0) && bulletCount > 0)
        {
            bulletCount--;
            BulletLoad();
            Instantiate(Bullet, transform.position, transform.rotation).owner = transform;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount > 0)
        {
            jumpCount--;
            rigidbody2D.linearVelocityY = 0;
            Destroy(Instantiate(GameManager.Instance.StepEffect, (Vector2)transform.position - new Vector2(0, 0.5f), Quaternion.identity), 1);
            rigidbody2D.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            rigidbody2D.linearVelocityY *= 0.5f;
        }

        float x = 0;
        if (Input.GetKey(KeyCode.A))
        {
            x -= Speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            x += Speed;
        }


        if (Input.GetMouseButtonDown(1) && time >= cool)
        {
            StartCoroutine(parring());
        }


        if (!dashing && !walling)
        {
            rigidbody2D.linearVelocityX = x;
        }

        if (isGround)
        {
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                rigidbody2D.linearVelocityX = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashtime >= dashcool)
        {
            StartCoroutine(dash());
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameManager.Instance.Flip();
        }
    }

    public ParticleSystem DashEffect;

    public void BulletLoad()
    {
        for (int i = 0; i < 10; i++)
        {
            BulletCount.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < bulletCount; i++)
        {
            BulletCount.GetChild(i).gameObject.SetActive(true);
        }
    }

    public IEnumerator parring()
    {
        DashEffect.gameObject.SetActive(false);
        DashEffect.gameObject.SetActive(true);
        DashEffect.loop = true;
        time = 0;
        Invin = 0.5f;
        Parring = true;
        Renderer.sprite = BoxSprite;
        Renderer.color = Color.black * 0.5f;
        yield return new WaitForSeconds(0.5f);
        Parring = false;
        Renderer.sprite = CircleSprite;
        Renderer.color = Color.black;
        DashEffect.loop = false;
    }

    IEnumerator dash()
    {
        DashEffect.gameObject.SetActive(false);
        DashEffect.gameObject.SetActive(true);
        DashEffect.loop = true;
        dashtime = 0;
        dashing = true;
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.AddForce((MouseP - (Vector2)transform.position).normalized * 18.5f, ForceMode2D.Impulse);
        Invin = 0.5f;
        transform.GetComponentInChildren<Collider2D>().enabled = false;
        Renderer.color = Color.black * 0.5f;
        yield return new WaitForSeconds(0.5f);
        transform.GetComponentInChildren<Collider2D>().enabled = true;
        Renderer.color = Color.black * 0.75f;
        yield return new WaitForSeconds(0.2f);
        Renderer.color = Color.black;
        dashing = false;
        DashEffect.loop = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            if (Invin<=0)
            {
                Instantiate(GameManager.Instance.DeathEffect, transform.position, Quaternion.Euler(0, 0, Quaternion.FromToRotation(Vector2.right, collision.rigidbody.position - (Vector2)transform.position).eulerAngles.z));
                Death();
                Destroy(rigidbody2D);
                GameManager.Instance.Gameover(0.1f);
                Destroy(Renderer.gameObject, 0.5f);
            }
            else if (Parring)
            {
                collision.rigidbody.GetComponent<Boss>().Stun(1f);
                collision.rigidbody.linearVelocity = Vector2.zero;
                collision.rigidbody.AddForce(((Vector2)transform.position - collision.rigidbody.position).normalized * Random.Range(3f, 10f) * -1, ForceMode2D.Impulse);
            }
        }
    }

    public void Death()
    {
        stone = true;
        Destroy(transform.GetComponentInChildren<Collider2D>());
        //GameManager.Instance.Gameover(0.1f);
        //Destroy(Renderer.gameObject, 0.5f);
    }

    public bool walling;

    public Transform BulletCount;
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (stone)
        {
            return;
        }
        if (collision.contacts[0].normal.y >= 0.7f && !isGround && collision.transform.CompareTag("Ground"))
        {
            jumpCount = 2;
            isGround = true;
            Destroy(Instantiate(GameManager.Instance.StepEffect, collision.contacts[0].point, Quaternion.identity), 1);
        }

        if (collision.contacts[0].normal.x == -1)
        {
            if (!GameManager.Instance.isFliped) walling = true;
            if (Input.GetKey(KeyCode.A)) walling = false;
            if (jumpCount < 2 && GameManager.Instance.isFliped)
            {
                jumpCount = 1;
            }
        }

        if (collision.contacts[0].normal.x == 1)
        {
            if (!GameManager.Instance.isFliped) walling = true;
            if (Input.GetKey(KeyCode.D)) walling = false;
            if (jumpCount < 2 && GameManager.Instance.isFliped)
            {
                jumpCount = 1;
            }
        }

        if (collision.contacts[0].normal.x != -1 && collision.contacts[0].normal.x != 1)
        {
            walling = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (walling)
        {
            walling = false;
        }
        isGround = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (stone)
        {
            return;
        }
        if (collision.transform.CompareTag("Enemy"))
        {
            if (Invin<=0)
            {
                Instantiate(GameManager.Instance.DeathEffect, transform.position, Quaternion.Euler(0, 0, Quaternion.FromToRotation(Vector2.right, collision.attachedRigidbody.position - (Vector2)transform.position).eulerAngles.z));
                Death();
                Destroy(rigidbody2D);
                GameManager.Instance.Gameover(0.1f);
                Destroy(Renderer.gameObject, 0.5f);
            }
        }
        if (collision.CompareTag("B"))
        {
            if (collision.attachedRigidbody.gravityScale == 0 && !collision.attachedRigidbody.GetComponent<Bullet>().owner)
            {
                if (bulletCount < 10)
                {
                    bulletCount++;
                    BulletLoad();
                    Destroy(collision.transform.parent.gameObject);
                }
            }
            else if (collision.attachedRigidbody.GetComponentInChildren<Bullet>().owner != transform)
            {
                if (Invin >0)
                {
                    if (Parring)
                    {
                        Instantiate(GameManager.Instance.ParringEffect, collision.attachedRigidbody.position, Quaternion.identity);
                        collision.attachedRigidbody.linearVelocity = Vector2.zero;
                        collision.attachedRigidbody.GetComponent<Bullet>().owner = transform;
                        collision.attachedRigidbody.AddForce(((Vector2)transform.position - collision.attachedRigidbody.position).normalized * Random.Range(10f, 25f) * -1, ForceMode2D.Impulse);
                        //collision.
                    }
                }
                else
                {
                    //if (collision.attachedRigidbody.GetComponent<Bullet>().Blade)
                    //{
                    //    
                    //}
                    //else
                    //{
                        Destroy(rigidbody2D);
                        Death();
                        transform.eulerAngles = new Vector3(0, -90, 0);
                        transform.parent = collision.transform.parent;
                        collision.attachedRigidbody.GetComponent<Bullet>().kill = true;
                    //}
                }
            }
        }
        if (collision.CompareTag("Blade"))
        {
            if (Invin<=0)
            {
                Instantiate(GameManager.Instance.DeathEffect, transform.position, Quaternion.Euler(0, 0, Quaternion.FromToRotation(Vector2.right, collision.attachedRigidbody.position - (Vector2)transform.position).eulerAngles.z));
                Death();
                Destroy(rigidbody2D);
                GameManager.Instance.Gameover(0.1f);
                Destroy(Renderer.gameObject, 0.5f);
            }
        }
    }
}
