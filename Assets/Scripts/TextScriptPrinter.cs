using Febucci.UI;
using Febucci.UI.Core;
using Febucci.UI.Core.Parsing;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class TextScript
{
    [TextArea]
    public string Texts;
    public Transform Target;
    public float waittime = 0;
    public bool cantskip=false;

    public bool stop;
}

public class TextScriptPrinter : MonoBehaviour
{
    public List<TextScript> ScriptText;

    public int PageNumber;

    public int Y_Pivot;

    public bool IsRight;

    public bool IsMiddle;

    [SerializeField] private RefreshSize RefreshSize;

    [SerializeField] private Transform TargetArrow;

    [SerializeField] private TextMeshProUGUI text;

    private TypewriterByCharacter typewriter;

    private TextAnimator_TMP textAnimator;

    public bool stop;

    public bool playing;

    public void NextText()
    {
            if (stop)
            {
                gameObject.SetActive(false);
                return;
            }

            if (playing)
            {
                if (!ScriptText[PageNumber].cantskip)
                {
                    typewriter.SkipTypewriter();
                    playing = false;
                }
            }
            else
            {
                gameObject.SetActive(true);
                StartCoroutine(Loadingtext());
            }
        
    }

    void OnMessage(EventMarker marker)
    {
        switch (marker.name)
        {
            case "end":
                playing = false;
                break;
            case "stop":
                typewriter.StopShowingText();
                break;
            case "Attack1":
                GameManager2.instance.View(1);
                break;
            case "Attack2":
                GameManager2.instance.View(2);
                break;
        }
    }

    void Awake()
    {
        RefreshSize = GetComponentInChildren<RefreshSize>();
        TargetArrow = transform.GetChild(1);
        text = GetComponentInChildren<TextMeshProUGUI>();
        textAnimator = text.GetComponent<TextAnimator_TMP>();
        typewriter = text.GetComponent<TypewriterByCharacter>();

        typewriter.onMessage.AddListener(OnMessage);

        gameObject.SetActive(false);
    }

    public IEnumerator Loadingtext()
    {
        playing = true;

        PageNumber++;

        if (ScriptText[PageNumber].stop)
        {
            gameObject.SetActive(false);
            stop = true;
            yield return null;
        }

        RefreshSize.IsRight = IsRight;
        if (IsMiddle)
        {
            RefreshSize.pRect.anchorMax = new Vector2(0.5f, 1);
            RefreshSize.pRect.anchorMin = new Vector2(0.5f, 1);
            RefreshSize.pRect.pivot = new Vector2(0.5f, 0);
        }
        else if (!IsRight)
        {
            RefreshSize.pRect.anchorMax = new Vector2(1, 1);
            RefreshSize.pRect.anchorMin = new Vector2(1, 1);
            RefreshSize.pRect.pivot = new Vector2(1, 0);
        }
        else
        {
            RefreshSize.pRect.anchorMax = new Vector2(0, 1);
            RefreshSize.pRect.anchorMin = new Vector2(0, 1);
            RefreshSize.pRect.pivot = new Vector2(0, 0);
        }


        yield return new WaitForSeconds(ScriptText[PageNumber].waittime);

        text.text = ScriptText[PageNumber].Texts+ "<?end>";

        transform.position = new Vector2(ScriptText[PageNumber].Target.position.x, ScriptText[PageNumber].Target.position.y + Y_Pivot);

    }
    public void EndText()
    {
        playing = false;
    }

    private void Update()
    {
        if (PageNumber != -1&& ScriptText[PageNumber].Target.gameObject.activeSelf)
        {
            if ((Vector2)TargetArrow.position != new Vector2(ScriptText[PageNumber].Target.position.x, ScriptText[PageNumber].Target.position.y + Y_Pivot))
            {
                TargetArrow.position = new Vector2(ScriptText[PageNumber].Target.position.x, ScriptText[PageNumber].Target.position.y + Y_Pivot);
            }
        }
    }

    public void Starting()
    {
        stop = false;
        gameObject.SetActive(true);
        NextText();
    }

    public void Reload()
    {
        PageNumber--;
        NextText();
    }
}
