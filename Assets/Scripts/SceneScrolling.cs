using UnityEngine;

public class SceneScrolling : MonoBehaviour
{
    [SerializeField] private float desktopScrollSpeed = 0.5f;
    [SerializeField] private float mobileScrollSpeed = 0.3f;
    [SerializeField] private SpriteRenderer background;

    private Camera mainCamera;
    private Vector3 lastMousePos;
    private Vector2 lastTouchPos;
    private float screenWidth; 
    private float backgroundHalfWidth;
    private bool isDragging;

    void Start()
    {
        mainCamera = Camera.main;
        screenWidth = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x
                      - mainCamera.ScreenToWorldPoint(Vector3.zero).x;

        backgroundHalfWidth = background.bounds.size.x / 2f;
    }

    private void Update()
    {
        if (isDragging) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#endif
    }

    //Прокрутка для десктопа
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 deltaPosition = currentMousePosition - lastMousePos;

            float moveX = deltaPosition.x * desktopScrollSpeed * Time.deltaTime;
            Vector3 newPos = transform.position - new Vector3(moveX, 0, 0);

            ClampAndApplyPosition(newPos);

            lastMousePos = currentMousePosition;
        }
    }

    //Прокрутка для телефона
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    lastTouchPos = touch.position;
                    break;

                case TouchPhase.Moved:
                    Vector2 deltaPosition = touch.position - lastTouchPos;

                    float moveX = deltaPosition.x * mobileScrollSpeed * Time.deltaTime;
                    Vector3 newPos = transform.position - new Vector3(moveX, 0, 0);

                    ClampAndApplyPosition(newPos);

                    lastTouchPos = touch.position;
                    break;
            }
        }
    }

    private void ClampAndApplyPosition(Vector3 newPosition)
    {
        float minX = -backgroundHalfWidth + screenWidth / 2;
        float maxX = backgroundHalfWidth - screenWidth / 2;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        transform.position = newPosition;
    }

    public void SetDragging(bool isDragging)
    {
        this.isDragging = isDragging;
    }
}
