using UnityEngine;

public class GravityDebug : MonoBehaviour
{
    string logText = "";

    void Update()
    {
        // 매 프레임 주요 값 확인
        logText =
            $"timeScale: {Time.timeScale}\n" +
            $"fixedDeltaTime: {Time.fixedDeltaTime}\n" +
            $"deltaTime: {Time.deltaTime}\n" +
            $"gravity2D: {Physics2D.gravity}\n" +
            $"targetFrameRate: {Application.targetFrameRate}\n" +
            $"vSyncCount: {QualitySettings.vSyncCount}\n" +
            $"FPS(대략): {(1f / Time.deltaTime):F1}";
    }

    void OnGUI()
    {
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 10, 600, 200), logText);
    }
}
