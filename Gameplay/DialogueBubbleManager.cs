using UnityEngine;

public class DialogueBubbleManager : MonoBehaviour
{
    [SerializeField] protected DialogueBubble bubblePrefab;
    [SerializeField] protected float bubbleLifetime = 5f;
    [SerializeField] protected float bubbleOffset = 1.5f;
    [SerializeField] protected bool screenSpacePositioning = true;
    [SerializeField] protected Vector2 screenOffset = new Vector2(0f, 100f);
    [SerializeField] protected Canvas uiCanvas;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
        }
    }

    public void ShowBubble(DialogueLine line, Transform npcTransform)
    {
        if (bubblePrefab == null) return;

        Vector3 spawnPosition;

        if (screenSpacePositioning && uiCanvas != null)
        {
            // Screen space positioning for better multi-screen support
            spawnPosition = GetScreenSpacePosition(npcTransform);
            var bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity, uiCanvas.transform);
            bubble.Initialize(line);
            bubble.SetScreenSpaceMode(true);
        }
        else
        {
            // World space positioning (original behavior)
            spawnPosition = npcTransform.position + Vector3.up * bubbleOffset;
            var bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
            bubble.Initialize(line);
            bubble.SetScreenSpaceMode(false);
        }

        Destroy(bubble.gameObject, bubbleLifetime);
    }

    private Vector3 GetScreenSpacePosition(Transform npcTransform)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return npcTransform.position + Vector3.up * bubbleOffset;
        }

        // Convert world position to screen point
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(npcTransform.position + Vector3.up * bubbleOffset);

        // Apply screen offset
        screenPoint += new Vector3(screenOffset.x, screenOffset.y, 0f);

        // Clamp to screen bounds
        screenPoint.x = Mathf.Clamp(screenPoint.x, 50f, Screen.width - 50f);
        screenPoint.y = Mathf.Clamp(screenPoint.y, 50f, Screen.height - 50f);

        // Convert back to world position for canvas
        if (uiCanvas != null && uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For overlay canvas, use screen coordinates directly
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out localPoint);
            return localPoint;
        }
        else
        {
            // For world space canvas, convert screen point to world
            screenPoint.z = Mathf.Abs(mainCamera.transform.position.z - npcTransform.position.z);
            return mainCamera.ScreenToWorldPoint(screenPoint);
        }
    }

    public void HideBubbles()
    {
        var bubbles = FindObjectsOfType<DialogueBubble>();
        foreach (var bubble in bubbles)
        {
            Destroy(bubble.gameObject);
        }
    }

    private void Update()
    {
        // Update camera reference if needed
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
}