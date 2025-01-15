using UnityEngine;
using DG.Tweening;

public class DragAndDrop : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip pickUpClip;
    [SerializeField] private AudioClip dropClip;

    [Header("Settings")]
    [SerializeField] private float placeOffset = 0.5f;
    [SerializeField] private float snapDuration = 0.5f;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private SceneScrolling sceneScrolling;

    private Vector3 offset;
    private Vector3 originalScale;
    private bool isDragging;
    private bool isPlaced = true;
    
    private void Start()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;

        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.GetComponent<AudioSource>();
        sceneScrolling = mainCamera.GetComponent<SceneScrolling>();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        isPlaced = false;

        sceneScrolling.SetDragging(isDragging);

        audioSource.PlayOneShot(pickUpClip);
        transform.DOKill();
        transform.DOScale(originalScale * 1.2f, 0.2f);

        offset = transform.position - GetMouseWorldPosition();
        ResetVelocity();
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        sceneScrolling.SetDragging(isDragging);

        if (!isPlaced)
        {
            rb.gravityScale = 2;
        }

        transform.DOScale(originalScale, 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlaced && !isDragging)
        {
            ResetVelocity();
            SnapToPlace();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isPlaced && !isDragging)
        {
            ResetVelocity();
            SnapToPlace();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = 10;

        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void SnapToPlace()
    {
        Collider2D placeCollider = Physics2D.OverlapCircle(transform.position, 0.8f, LayerMask.GetMask("Place"));
        if (placeCollider == null) return;

        isPlaced = true;

        Vector3 clampedPosition;
        Vector3 targetPosition;

        if (placeCollider.CompareTag("Place"))
        {
            Vector3 shelfCenter = placeCollider.bounds.center;
            targetPosition = (transform.position + shelfCenter) / 2;
        }
        else
        {
            if (!isDragging)
                targetPosition = transform.position;
            else
                targetPosition = GetMouseWorldPosition();
        }
 
        clampedPosition = ClampToBounds(placeCollider, targetPosition);

        transform.DOMove(clampedPosition, snapDuration)
                 .SetEase(Ease.OutCubic);

        audioSource.PlayOneShot(dropClip);

    }

    private Vector3 ClampToBounds(Collider2D shelfCollider, Vector3 targetPosition)
    {
        Bounds shelfBounds = shelfCollider.bounds;

        float clampedX = Mathf.Clamp(targetPosition.x, shelfBounds.min.x, shelfBounds.max.x);
        float clampedY = Mathf.Clamp(targetPosition.y, shelfBounds.min.y + placeOffset , shelfBounds.max.y + placeOffset);

        return new Vector3(clampedX, clampedY, -1f);
    }

    private void ResetVelocity()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }
}