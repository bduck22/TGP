using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class RefreshSize : MonoBehaviour
{
    public RectTransform pRect;

    public bool IsRight;

    private void Awake()
    {
        pRect = GetComponent<RectTransform>();
    }
    private void Start()
    {
    }


    public void fresh()
    {
        //Vector3[] corners = new Vector3[4];
        //pRect.GetWorldCorners(corners);

        //// 코너 월드 좌표를 화면 픽셀 좌표로 변환
        //Vector3 bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        //Vector3 topRightScreen = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);

        //Vector2 screenCorrection = Vector2.zero;

        //// 1. 이탈 거리 계산 (오른쪽과 왼쪽)
        //float rightDistance = topRightScreen.x - Screen.width;
        //float leftDistance = 0 - bottomLeftScreen.x;

        //// 2. 이탈 거리 계산 (위쪽과 아래쪽)
        //float topDistance = topRightScreen.y - Screen.height;
        //float bottomDistance = 0 - bottomLeftScreen.y;

        //bool isOffScreen = false;

        //// --- 이탈 픽셀 확인 및 디버그 로직 ---

        //if (rightDistance > 0)
        //{
        //    Debug.Log($" 오른쪽으로 {rightDistance:F2} 픽셀 벗어남");
        //    screenCorrection.x -= rightDistance;
        //    isOffScreen = true;
        //}

        //if (leftDistance > 0)
        //{
        //    Debug.Log($" 왼쪽으로 {leftDistance:F2} 픽셀 벗어남");
        //    screenCorrection.x += leftDistance;
        //    isOffScreen = true;
        //}

        //if (topDistance > 0)
        //{
        //    Debug.Log($" 위쪽으로 {topDistance:F2} 픽셀 벗어남");
        //    screenCorrection.y -= topDistance;
        //    isOffScreen = true;
        //}

        //if (bottomDistance > 0)
        //{
        //    Debug.Log($" 아래쪽으로 {bottomDistance:F2} 픽셀 벗어남");
        //    screenCorrection.y += bottomDistance;
        //    isOffScreen = true;
        //}

        //if (!isOffScreen)
        //{
        //    Debug.Log("현재 화면 안에 있습니다.");
        //}


        //float canvasScaleFactor = pRect.lossyScale.x;

        //if (Mathf.Abs(canvasScaleFactor) < float.Epsilon)
        //{
        //    Debug.LogWarning("Canvas Scale Factor가 0에 가까워 클램핑을 건너뜁니다.");
        //    return;
        //}


        LayoutRebuilder.ForceRebuildLayoutImmediate(pRect);
    }
}
