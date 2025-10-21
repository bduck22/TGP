using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
public enum Boss_Status
{
    White,
    Black
}


public class Boss : MonoBehaviour
{
    public PlayerController Target;

    private Rigidbody2D rigidbody2D;

    public float Speed;
    public float Range;

    public bool isGround;
    public int jumpCount;
    public float jumpPower;

    public bool doublejumptrigger;

    public Boss_Status status;
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        status = Boss_Status.White;
        patterntime = Random.Range(3f, 6f);
        Collider = GetComponentInChildren<Collider2D>();
    }

    public float time;

    public bool change;

    public float Hp;

    public Bullet Bullet;


    public void Pattern()
    {
        patterned = true;
        patterntime += Random.Range(3f, 6f - (10 - Hp) * 0.2f);
        if (status == Boss_Status.White)
        {
            StartCoroutine(WhitePattern(Random.Range(0, 5)));
        }
        else
        {
            StartCoroutine(BlackPattern(Random.Range(0, 5)));
        }
    }
    IEnumerator WhitePattern(int type)
    {
        switch (type)
        {
            case 0:
                Jump();
                yield return new WaitForSeconds(0.85f);
                for (int i = 0; i < 5; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.owner = transform;
                    B.transform.LookAt(Target.transform);
                    yield return new WaitForSeconds(0.2f);
                }
                AllLock = true;
                Aura.gameObject.SetActive(true);
                yield return new WaitForSeconds(2f);
                GameObject Bl = Instantiate(Blade, transform.position, Quaternion.identity);
                Bl.transform.right = ((transform.position - Target.transform.position).normalized);
                AllLock = false;
                Aura.gameObject.SetActive(false);
                break;
            case 1:
                for (int i = 0; i < 45; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.eulerAngles = new Vector3(i * 8, -90, 0);
                    B.owner = transform;
                    yield return new WaitForSeconds(0.02f);
                }
                break;
            case 2:
                for (int i = 0; i < 7; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.LookAt(Target.transform);
                    B.transform.Rotate(i * 5 - 15, 0, 0);
                    B.Shoot();
                    B.owner = transform;
                }
                yield return new WaitForSeconds(0.2f);
                for (int i = 0; i < 7; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.LookAt(Target.transform);
                    B.transform.Rotate(i * 5 - 15, 0, 0);
                    B.Shoot();
                    B.owner = transform;
                }
                break;
            case 3:
                AllLock = true;
                yield return new WaitForSeconds(1f);
                for (int i = 0; i < 23; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.eulerAngles = new Vector3(i * -8, -90, 0);
                    B.owner = transform;
                }
                yield return new WaitForSeconds(0.35f);
                for (int i = 0; i < 23; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.eulerAngles = new Vector3(i * -8, -90, 0);
                    B.owner = transform;
                    B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.eulerAngles = new Vector3(180 - (i * -8), -90, 0);
                    B.owner = transform;
                    yield return new WaitForSeconds(0.1f);
                }
                AllLock = false;
                break;
            case 4:
                Stun(2);
                AllLock = true;
                yield return new WaitForSeconds(2f);
                AllLock = false;
                for (int i = 0; i < 25; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.owner = transform;
                    B.transform.LookAt(Target.transform);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
        }
        patterned = false;
    }

    public Transform Aura;
    public GameObject Blade;
    IEnumerator BlackPattern(int type)
    {
        switch (type)
        {
            case 0:
                Teleport();
                AllLock = true;
                yield return new WaitForSeconds(0.3f);
                Collider.isTrigger = true;
                rigidbody2D.linearVelocityY = -30;
                break;
            case 1:
                AllLock = true;
                Aura.gameObject.SetActive(true);
                yield return new WaitForSeconds(2f);
                GameObject Bl = Instantiate(Blade, transform.position, Quaternion.identity);
                Bl.transform.right = ((transform.position - Target.transform.position).normalized);
                AllLock = false;
                Aura.gameObject.SetActive(false);
                break;
            case 2:
                Teleport();
                yield return new WaitForSeconds(0.5f);
                for (int i = 0; i < 10; i++)
                {
                    Bullet B = Instantiate(Bullet, transform.position, Quaternion.identity);
                    B.transform.eulerAngles = new Vector3(i * 36, -90, 0);
                    B.owner = transform;
                }
                break;
            case 3:
                AllLock = true;
                yield return new WaitForSeconds(0.5f);
                Collider.isTrigger = true;
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.AddForce((Target.transform.position - transform.position).normalized * 40, ForceMode2D.Impulse);
                break;
            case 4:
                Stun(2);
                AllLock = true;
                Aura.gameObject.SetActive(true);
                yield return new WaitForSeconds(2f);
                for (int i = 0; i < 3; i++)
                {
                    GameObject Bl2 = Instantiate(Blade, transform.position, Quaternion.identity);
                    Bl2.transform.right = ((transform.position - Target.transform.position).normalized);
                    yield return new WaitForSeconds(1.25f);
                }
                AllLock = false;
                Aura.gameObject.SetActive(false);
                break;
        }
        patterned = false;
    }

    public bool patterned;
    void Teleport()
    {
        Instantiate(InEffect, transform.position, Quaternion.identity);
        rigidbody2D.linearVelocityX = 0;
        transform.position = (Vector2)Target.transform.position + new Vector2(0, 2);
        rigidbody2D.linearVelocityY = 7;
        Instantiate(OutEffect, transform.position, Quaternion.identity);
    }

    public GameObject InEffect;
    public GameObject OutEffect;

    public int patterncount;

    public float patterntime;

    public bool AllLock;

    public Collider2D Collider;

    public Transform DashEffect;

    public TMP_Text T;

    public GameObject Hit;

    public bool opening;

    public Transform Clear;
    void Update()
    {
        if(Hp <= 0&&start)
        {
            AllLock = true;
            start = false;
            GetComponent<AudioSource>().clip = DeadSound;
            GetComponent<AudioSource>().loop = false;
            GetComponent<AudioSource>().Play();
            Clear.gameObject.SetActive(true);
            Target.stone = true;
        }

        if (!start)
        {
            if(transform.position.y <= 0 && !opening&&!Camera.main.GetComponent<Animator>().enabled)
            {
                Time.timeScale = 0.025f;
                Camera.main.GetComponent<Animator>().enabled = true;
            }
            if(transform.position.y <=-1.5f&&!opening)
            {
                Camera.main.GetComponent<Animator>().SetTrigger("Out");
                opening = true;
                GameManager.Instance.Flip();
                StartCoroutine(startsound());
                GameManager.Instance.GetComponent<AudioSource>().enabled = false;
                Time.timeScale = 1;
            }
        }

        if (GameManager.Instance.isGameover)
        {
            if (rigidbody2D.linearVelocityY > 4f)
            {
                doublejumptrigger = true;
            }
            Jump();
        }

        if (GameManager.Instance.isGameover && !T.gameObject.activeSelf)
        {
            GetComponent<AudioSource>().clip = DeadSound;
            GetComponent<AudioSource>().loop = false;
            GetComponent<AudioSource>().Play();
            DashEffect.gameObject.SetActive(false);
            T.gameObject.SetActive(true);
            switch (Random.Range(0, 11))
            {
                case 0:
                    T.text = "?어디감";
                    break;
                case 1:
                    T.text = "ㅋ.";
                    break;
                case 2:
                    T.text = "허접";
                    break;
                case 3:
                    T.text = "강해져서 돌아와라";
                    break;
                case 4:
                    T.text = "Eeeeeeeez";
                    break;
                case 5:
                    T.text = "이게 어렵나?";
                    break;
                case 6:
                    T.text = "너 다음 픽업 픽뚫남";
                    break;
                case 7:
                    T.text = "궁서체";
                    break;
                case 8:
                    T.text = "미안하다 이거 보여주려고 어그로끌었다.. \n나루토 사스케 싸움수준 ㄹㅇ실화냐?\n 진짜 세계관최강자들의 싸움이다..";
                    break;
                case 9:
                    T.text = "WA!";
                    break;
                case 10:
                    T.text = "진짜 뭐하냐";
                    break;
            }
        }

        if (transform.position.y < -8.75f)
        {
            Collider.isTrigger = false;
            AllLock = false;
        }

        if (start&&!GameManager.Instance.isGameover)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetMouseButtonDown(2))
                {
                    Hp = 1;
                    HpText.text = Hp.ToString("#,##0");
                }
            }
        }

        if (start && !stuned && !GameManager.Instance.isGameover)
        {
            DashEffect.gameObject.SetActive(Collider.isTrigger);
            if (Collider.isTrigger)
            {
                DashEffect.up = -((rigidbody2D.linearVelocity).normalized);
            }

            if (Lock)
            {
                if (rigidbody2D.linearVelocityY <= 0)
                {
                    Lock = false;
                }
            }

            StunEffect.gameObject.SetActive(false);
            Moving();
            if (rigidbody2D.linearVelocityY > 4f)
            {
                doublejumptrigger = true;
            }
            if (time > 15 || patterncount > 5)
            {
                change = true;
            }
            else
            {
                if (patterntime < time && !patterned)
                {
                    patterncount++;
                    Pattern();
                }
                time += Time.deltaTime;
            }



            if (status == Boss_Status.White)
            {
                if (GameManager.Instance.isFliped && change)
                {
                    Hp ++;
                    patterncount = 0;
                    patterntime = Random.Range(3f, 6f - (10 - Hp) * 0.2f);
                    change = false;
                    time = 0;
                    GameManager.Instance.Flip();
                    status = Boss_Status.Black;
                    Range = 0;
                    Speed = 4.5f;
                    jumpPower = 10;
                    HpText.text = Hp.ToString("#,##0");
                }
            }
            else if (status == Boss_Status.Black)
            {
                if (!GameManager.Instance.isFliped && change)
                {
                    Hp ++;
                    patterncount = 0;
                    patterntime = Random.Range(3f, 6f - (10 - Hp) * 0.2f);
                    change = false;
                    time = 0;
                    GameManager.Instance.Flip();
                    status = Boss_Status.White;
                    Range = 3;
                    Speed = 3;
                    jumpPower = 7;
                    HpText.text = Hp.ToString("#,##0");
                }
            }
        }
        else
        {
            if (stuned) StunEffect.gameObject.SetActive(true);

            //if(!stuned&&start) rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
    public Transform StunEffect;

    public bool start;

    public bool stuned;

    public void Moving()
    {
        float x = 0;
        if (Target.transform.position.x < transform.position.x)
        {
            x -= Speed;
        }
        if (Target.transform.position.x > transform.position.x)
        {
            x += Speed;
        }

        if (!Lock && !AllLock)
        {
            if (Vector2.Distance(transform.position, Target.transform.position) < Range - 0.5f && (!walling || status == Boss_Status.White))
            {
                rigidbody2D.linearVelocityX = -x;
            }
            else if (Vector2.Distance(transform.position, Target.transform.position) >= Range && (!walling || status == Boss_Status.White))
            {
                rigidbody2D.linearVelocityX = x;
            }
            else
            {
                rigidbody2D.linearVelocityX = 0;
            }
            if (Target.transform.position.y > transform.position.y + 1f)
            {
                Jump();
            }
        }
    }
    public bool Lock;

    public Vector3 CP;
    public void Jump()
    {
        if ((jumpCount == 2 && isGround) || (jumpCount == 1 && doublejumptrigger && rigidbody2D.linearVelocityY < 2 && !walling))
        {
            if (!AllLock)
            {
                jumpCount--;
                rigidbody2D.linearVelocityY = 0;
                rigidbody2D.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
                Destroy(Instantiate(GameManager.Instance.StepEffect, (Vector2)transform.position - new Vector2(0, 0.75f), Quaternion.identity), 1);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!start && !isGround && collision.transform.CompareTag("Ground")&&!AllLock)
        {
            isGround = true;
            start = true;
            Camera.main.GetComponent<Animator>().enabled = false;
            Camera.main.transform.rotation = Quaternion.identity;
            Camera.main.orthographicSize = 10;
            Camera.main.transform.position = CP;
            Destroy(Instantiate(GameManager.Instance.StepEffect, collision.contacts[0].point, Quaternion.identity), 1);
        }
    }
    IEnumerator startsound()
    {
        Collider.GetComponent<AudioSource>().enabled = true;
        yield return new WaitForSeconds(1);
        GetComponent<AudioSource>().enabled = true;
    }

    public bool walling;

    public GameObject ParringOb;

    public TMP_Text HpText;

    public AudioClip DeadSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            if (Collider.isTrigger)
            {
                if (!collision.GetComponent<PlatformEffector2D>())
                {
                    Collider.isTrigger = false;
                    AllLock = false;
                }
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("B"))
        {
            if (collision.attachedRigidbody)
            {
                if (stuned)
                {
                    if (collision.attachedRigidbody.GetComponent<Bullet>().owner != transform && collision.attachedRigidbody.GetComponent<Bullet>().owner && start)
                    {
                        collision.attachedRigidbody.GetComponent<Bullet>().owner = null;
                        collision.attachedRigidbody.gravityScale = 0;
                        if (!AllLock) stuned = false;
                        Hp--;
                        Instantiate(Hit, transform.position, Quaternion.identity);
                        HpText.text = Hp.ToString("#,##0");
                    }
                }
                else if (collision.attachedRigidbody.GetComponent<Bullet>().owner != transform)
                {
                    if (collision.attachedRigidbody.gravityScale != 0)
                    {
                        Stun(0.6f);
                        if (status == Boss_Status.White)
                        {
                            Destroy(collision.attachedRigidbody.GetComponent<Collider2D>());
                            Destroy(collision.attachedRigidbody.gameObject);
                            Destroy(Instantiate(ParringOb, collision.attachedRigidbody.position, Quaternion.Euler(0, 0, Random.Range(1f, 180f))), 3);
                        }
                        else
                        {
                            Instantiate(GameManager.Instance.ParringEffect, collision.attachedRigidbody.position, Quaternion.identity);
                            collision.attachedRigidbody.linearVelocity = Vector2.zero;
                            collision.attachedRigidbody.GetComponent<Bullet>().owner = transform;
                            collision.attachedRigidbody.AddForce(((Vector2)transform.position - collision.attachedRigidbody.position).normalized * Random.Range(10f, 25f) * -1, ForceMode2D.Impulse);
                        }
                    }
                    else
                    {
                        //Debug.Log("바닥 막대 잡아 쏘기 및 꽃힌 막대 잡아 쏘기");
                        //바닥 막대 잡아 쏘기 및 꽃힌 막대 잡아 쏘기-----------------------------------------흑색때는 핀 못 집게
                        collision.attachedRigidbody.gravityScale = 0;
                        if (collision.attachedRigidbody.bodyType != RigidbodyType2D.Kinematic&&status!=Boss_Status.Black)
                        {
                            collision.attachedRigidbody.transform.parent = transform.GetChild(2).transform;
                            collision.attachedRigidbody.bodyType = RigidbodyType2D.Kinematic;
                        }
                        
                        if(collision.attachedRigidbody.bodyType == RigidbodyType2D.Kinematic || status == Boss_Status.White)
                        {
                            StartCoroutine(GetShoot(collision.attachedRigidbody));
                        }
                    }
                }
            }
        }
    }

    IEnumerator GetShoot(Rigidbody2D rb)
    {
        if (!shoot && !AllLock)
        {
            shoot = true;
        }
        else
        {
            yield break;
        }
        yield return new WaitForSeconds(0.75f);
        if (!rb)
        {
            shoot = false;
            yield break;
        }
        rb.GetComponent<Bullet>().owner = transform;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.transform.parent = null;
        rb.transform.position = transform.position;
        rb.transform.LookAt(Target.transform);
        rb.GetComponent<Bullet>().Shoot();
        rb.gravityScale = 2;
        shoot = false;
    }

    public bool shoot;

    public void Stun(float time)
    {
        StartCoroutine(stun(time));
    }

    IEnumerator stun(float time)
    {
        stuned = true;
        yield return new WaitForSeconds(time);
        stuned = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {

        if (collision.contacts[0].normal.x == 0 && !isGround && collision.transform.CompareTag("Ground"))
        {
            Destroy(Instantiate(GameManager.Instance.StepEffect, collision.contacts[0].point, Quaternion.identity), 1);
            doublejumptrigger = false;
            jumpCount = 2;
            isGround = true;
        }


        if (collision.contacts[0].normal.x == -1)
        {
            walling = true;
            if (status == Boss_Status.White)
            {
                jumpCount = 1;
                isGround = true;
                if (Target.transform.position.y > transform.position.y + 1)
                {
                    walling = false;
                    rigidbody2D.linearVelocityX = -Speed;
                    Jump();
                    Lock = true;
                }

            }

        }

        if (collision.contacts[0].normal.x == 1)
        {
            walling = true;
            if (status == Boss_Status.White)
            {
                jumpCount = 1;
                isGround = true;
                if (Target.transform.position.y > transform.position.y + 1)
                {
                    walling = false;
                    rigidbody2D.linearVelocityX = Speed;
                    Jump();
                    Lock = true;
                }

            }
        }

        if (collision.contacts[0].normal.x != -1 && collision.contacts[0].normal.x != 1)
        {
            walling = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Wall"))
        {
            walling = false;
        }
        isGround = false;
    }
}
