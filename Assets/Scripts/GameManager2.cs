using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class GameManager2 : MonoBehaviour
{
    public GameManager GameManager1;

    public PlayableDirector playableDirector;

    public List<TimelineAsset> timelines;

    public static GameManager2 instance;

    public Transform[] offs;

    public PlayerController Player;

    public int num;

    public Transform ClickText;

    public Transform MobSpawner;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

    }

    public void ongame()
    {
        foreach(Transform t in offs)
        {
            t.gameObject.SetActive(false);
        }
        Player.gameObject.SetActive(true);
        Player.stone = false;

        MobSpawner.gameObject.SetActive(true);

        GetComponent<AudioSource>().Play();
    }

    public bool GameStart = false;

    public void View(int linenum)
    {
        num = linenum;
        playableDirector.time = 0;
        playableDirector.playableAsset = timelines[linenum];
        playableDirector.Play();
    }

    public void Skip()
    {
        playableDirector.time = playableDirector.duration;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && !GameStart)
        {
            ClickText.gameObject.SetActive(false);
            GameStart = true;
            GameManager1.gameObject.SetActive(true);
            View(0);
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
        {
            if (playableDirector.state == PlayState.Playing && num==0)
            {
                Skip();
            }
        }

        if (GameStart && Input.GetKeyUp(KeyCode.P)){
            GameManager.Instance.textScriptPrinter.PageNumber = 7;
            Skip();
            ongame();
        }
    }
}
