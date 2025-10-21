using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pin : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    private bool _isPinned = false;
    private bool _isLaunched = false;

    public GameObject Slash;
    
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (!_isPinned && _isLaunched)
        {
            transform.parent.position += Vector3.up * moveSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance.isGameOver)
        {
            return;
        }
        _isPinned = true;
        if (other.gameObject.CompareTag("Target"))
        {
            GameManager.Instance.DecreaseGoal();
            //GameManager.Instance.Flip(1);
            transform.parent.GetComponent<AudioSource>().enabled = true;
            StartCoroutine(slash());
            if (!GameManager.Instance.isGameOver)
            {
                GameManager.Instance.DecreaseGoal();
            }
        }
        else if (other.gameObject.CompareTag("Pin"))
        {
            GameManager.Instance.SetGameOver(false);
        }
    }

    IEnumerator slash()
    {
        yield return new WaitForSeconds(1f);
        CameraSet.Instance.Target = transform;
        CameraSet.Instance.Follow = true;
        Camera.main.orthographicSize = 10f;
        Destroy(Instantiate(Slash, transform.position, Slash.transform.rotation), 3f);
        transform.parent.GetComponent<Rigidbody2D>().gravityScale = 1;
        transform.parent.GetComponent<Rigidbody2D>().AddForce(transform.up * -25, ForceMode2D.Impulse);
        StartCoroutine(transform.parent.GetComponent<PlayerController>().parring());
        transform.GetComponentInChildren<Collider2D>().isTrigger = true;
        Destroy(GameManager.Instance.GetComponent<SpriteRenderer>(), 2f);
        yield return new WaitForSeconds(1.8f);
        GameManager.Instance.Enemy.gameObject.SetActive(true);
        transform.GetComponentInChildren<Collider2D>().isTrigger = false;
        transform.parent.GetComponent<PlayerController>().stone = false;
        Destroy(this);
    }

    public void Launch()
    {
        _isLaunched = true;
    }
    
}
