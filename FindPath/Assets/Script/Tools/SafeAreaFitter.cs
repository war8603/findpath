using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rt;
    private Rect _lastSafeArea;
    private readonly RectOffset _reserved = new(); // left, right, top, bottom (px)
    private readonly RectOffset _lastReserved = new(); // 변경 감지용

    // (선택) 화면 회전 감지용
    private ScreenOrientation _lastOrientation;
    
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        // SafeArea 변경, 회전 변경, Reserved 변경 시에만 갱신
        if (_lastSafeArea != Screen.safeArea ||
            _lastOrientation != Screen.orientation ||
            !ReservedEquals(_reserved, _lastReserved))
        {
            Apply();
        }
    }
    
    /// <summary>
    /// 배너/툴바 등으로 제외하고 싶은 픽셀 여백을 설정
    /// 값은 "스크린 픽셀" 기준입니다.
    /// </summary>
    public void SetReservedInsets(int leftPx = 0, int topPx = 0, int rightPx = 0, int bottomPx = 0)
    {
        _reserved.left = Mathf.Max(0, leftPx);
        _reserved.top = Mathf.Max(0, topPx);
        _reserved.right = Mathf.Max(0, rightPx);
        _reserved.bottom = Mathf.Max(0, bottomPx);
        Apply(); // 즉시 반영
    }

    private void Apply()
    {
        var sa = Screen.safeArea;
        _lastSafeArea = sa;
        _lastOrientation = Screen.orientation;
        _lastReserved.left   = _reserved.left;
        _lastReserved.top    = _reserved.top;
        _lastReserved.right  = _reserved.right;
        _lastReserved.bottom = _reserved.bottom;

        // ✅ Canvas 기준 픽셀 영역
        var canvas = _rt.GetComponentInParent<Canvas>();
        var root   = canvas != null ? canvas.rootCanvas : null;
        var pxRect = root != null ? root.pixelRect : new Rect(0, 0, Screen.width, Screen.height);

        // SafeArea를 Canvas 픽셀좌표로 보정
        var x = sa.x + _reserved.left   - pxRect.x;
        var y = sa.y + _reserved.bottom - pxRect.y;
        var w = sa.width  - (_reserved.left + _reserved.right);
        var h = sa.height - (_reserved.top  + _reserved.bottom);

        // 음수/넘침 방지
        w = Mathf.Max(0, w);
        h = Mathf.Max(0, h);

        // ✅ Canvas.pixelRect로 정규화
        var anchorMin = new Vector2(x / pxRect.width,  y / pxRect.height);
        var anchorMax = new Vector2((x + w) / pxRect.width, (y + h) / pxRect.height);

        // 경계 클램프
        anchorMin = Vector2.Max(Vector2.zero, anchorMin);
        anchorMax = Vector2.Min(Vector2.one,  anchorMax);

        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }
    
    private static bool ReservedEquals(RectOffset a, RectOffset b)
    {
        return a.left == b.left && a.right == b.right && a.top == b.top && a.bottom == b.bottom;
    }
}