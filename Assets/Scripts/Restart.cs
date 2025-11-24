using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1;
            GameManager.Instance.ScreenShader.SetFloat("_Fliped", 0);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
