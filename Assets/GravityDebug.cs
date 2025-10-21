using UnityEngine;

public class GravityDebug : MonoBehaviour
{
    string logText = "";

    void Update()
    {
        // �� ������ �ֿ� �� Ȯ��
        logText =
            $"timeScale: {Time.timeScale}\n" +
            $"fixedDeltaTime: {Time.fixedDeltaTime}\n" +
            $"deltaTime: {Time.deltaTime}\n" +
            $"gravity2D: {Physics2D.gravity}\n" +
            $"targetFrameRate: {Application.targetFrameRate}\n" +
            $"vSyncCount: {QualitySettings.vSyncCount}\n" +
            $"FPS(�뷫): {(1f / Time.deltaTime):F1}";
    }

    void OnGUI()
    {
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 10, 600, 200), logText);
    }
}
