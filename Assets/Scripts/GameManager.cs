using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public bool isGameOver = false;
    public int goal;

    [SerializeField] private GameObject btnRetry;
    [SerializeField] private Transform textGoal;
    [SerializeField] private Color red;

    [SerializeField] private Renderer2DData renderer2DData;
    [SerializeField] private string featureName = "Fullscreen Pass Renderer Feature";
    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance == null)
        {
            Instance = this;
        }
        foreach (var f in renderer2DData.rendererFeatures)
        {
            if (f.name == featureName && f is FullScreenPassRendererFeature fsFeature)
            {
                ScreenShader = fsFeature.passMaterial;
                break;
            }
        }
    }

    public bool season1;
    void Start()
    {
        if (!season1)
        {
            //Cursor.visible = false;
            StartCoroutine(flip(0));
        }
        //ScreenShader.SetFloat("_Fliped", 1.5f);
    }

    public Transform Map;

    public Transform Restart;

    public GameObject DeathEffect;
    public GameObject StepEffect;
    public GameObject ParringEffect;
    public GameObject ThumpEffect;

    public AudioSource source;

    public AudioClip Pang;
    public AudioClip Parry;

    public void DecreaseGoal()
    {
        SetGameOver(true);
    }

    public bool isFliped;

    public void Flip(float time)
    {
        StartCoroutine(flip(time));
    }
    public void Flip()
    {
        StartCoroutine(flip(0));
    }

    [SerializeField] private float flipValue;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Application.Quit();
        }
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
        {
            NextText();
        }
    }

    public IEnumerator flip(float time)
    {
        yield return new WaitForSeconds(time);
        if (Time.timeScale == 1) Time.timeScale = 0.75f;
        isFliped = !isFliped;
        for (flipValue = (isFliped ? 0 : 1); (isFliped ? flipValue <= 1.5f : flipValue >= 0);)
        {
            if (isFliped)
            {
                flipValue += 0.075f;
            }
            else
            {
                flipValue -= 0.075f;
            }
            ScreenShader.SetFloat("_Fliped", flipValue);
            yield return new WaitForSeconds(0.01f);
        }
        if (isFliped)
        {
            flipValue = 1.5f;
        }
        else
        {
            flipValue = 0;
        }
        ScreenShader.SetFloat("_Fliped", flipValue);
        if (Map)
        {
            Map.gameObject.SetActive(!isFliped);
        }
        Time.timeScale = 1f;
    }
    public void SetGameOver(bool success)
    {
        if (isGameOver == false)
        {
            isGameOver = true;
            if (success)
            {
                GetComponent<AudioSource>().enabled = true;
                //Camera.main.backgroundColor = Color.black;
                //transform.GetComponent<SpriteRenderer>().color = Color.white;
                transform.GetComponent<TargetCircle>().enabled = false;
                textGoal.gameObject.SetActive(false);
            }
        }
    }

    public Transform GameoverText;
    public void Gameover(float time)
    {
        StartCoroutine(gameover(time));
    }

    IEnumerator gameover(float time)
    {
        yield return new WaitForSeconds(time);
        Flip();
        yield return new WaitForSeconds(0.4f);
        Flip();
        GameoverText.gameObject.SetActive(true);
        isGameover = true;
    }

    public bool isGameover;

    private void OnApplicationQuit()
    {
        ScreenShader.SetFloat("_Fliped", 0f);
    }


    public Transform Enemy;
    public Material ScreenShader;

    //public void colored()
    //{
    //    Camera.main.backgroundColor += Color.white / 4f* Time.deltaTime- Color.black;
    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        foreach (SpriteRenderer rb in transform.GetComponentsInChildren<SpriteRenderer>())
    //        {
    //            rb.color -= Color.white /4f * Time.deltaTime;
    //            rb.color += Color.black;
    //        }
    //    }
    //}

    private void ShowRetryButton()
    {
        btnRetry.SetActive(true);
    }

    public void Retry()
    {
        SceneManager.LoadScene(0);
    }

    public TextScriptPrinter textScriptPrinter;

    public void NextText()
    {
        if (textScriptPrinter)
        {
            textScriptPrinter.NextText();
        }
    }
}
