using UnityEngine;

public class Bullet : MonoBehaviour
{
    public AudioClip Breake;
    public AudioClip Plugin;
    public AudioClip BossHit;

    public AudioSource source;

    public Transform owner;

    private Rigidbody2D rigidbody2D;
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        
        Shoot();
    }

    public GameObject ParringEffect;

    public GameObject WalledEffect;

    void Update()
    {
        if (transform.position.y <= -15)
        {
            Destroy(gameObject);
        }
        if (rigidbody2D&&rigidbody2D.gravityScale!=0)
        {
            transform.LookAt((Vector2)transform.position + rigidbody2D.linearVelocity.normalized);
        }
    }

    public void Shoot()
    {
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.AddForce(transform.forward * 35f, ForceMode2D.Impulse);
        transform.LookAt((Vector2)transform.position + rigidbody2D.linearVelocity.normalized);
    }

    public bool kill;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Blade"))
        {
            if (owner)
            {
                Instantiate(ParringEffect, transform.position, Quaternion.identity);
                GameManager.Instance.source.clip = Breake;
                GameManager.Instance.source.Play();
                Destroy(gameObject);
            }
        }
        if (collision.transform.CompareTag("B"))
        {
            if (collision.attachedRigidbody && !kill)
            {
                if(((owner && collision.attachedRigidbody.GetComponent<Bullet>().owner) 
                    && owner != collision.attachedRigidbody.GetComponent<Bullet>().owner)
                    || (!owner&& !collision.transform.root.GetComponent<Boss>())
                    || (owner&& !collision.attachedRigidbody.GetComponent<Bullet>().owner&&!collision.transform.root.GetComponent<Boss>()))
                {
                    Instantiate(ParringEffect, transform.position, Quaternion.identity);
                    GameManager.Instance.source.clip = Breake;
                    GameManager.Instance.source.Play();
                    Destroy(gameObject);
                }
            }
        }
        else if(rigidbody2D.gravityScale != 0)
        {
            if (collision.transform.parent.transform != owner)
            {
                if(!collision.CompareTag("Pin"))
                {
                    if (collision.transform.CompareTag("Enemy")&&collision.attachedRigidbody.GetComponent<Boss>().stuned&&collision.attachedRigidbody.GetComponent<Boss>().start && !kill)
                    {
                        source.clip = BossHit;
                        source.Play();
                        transform.parent = collision.transform;
                        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                    }
                    else if (!collision.transform.CompareTag("Enemy")&&!collision.CompareTag("Pin"))
                    {
                        source.clip = Plugin;
                        source.Play();
                        owner = null;
                        transform.parent = collision.transform.parent;
                        rigidbody2D.linearVelocity = Vector2.zero;
                        rigidbody2D.gravityScale = 0;
                        Instantiate(WalledEffect, transform.position, Quaternion.identity);
                        if (kill)
                        {
                            if (transform.GetComponentInChildren<PlayerController>())
                            {
                                GameManager.Instance.Gameover(0.1f);
                                Destroy(transform.GetComponentInChildren<PlayerController>().GetComponentInChildren<SpriteRenderer>().gameObject, 0.5f);
                            }
                        }
                    }
                }
            }
        }
    }
}
