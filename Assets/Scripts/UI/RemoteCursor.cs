using UnityEngine;

public class RemoteCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;

    // 이미지 기준 실제 클릭 지점
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Cursor.SetCursor(
            cursorTexture,
            hotSpot,
            CursorMode.Auto
        );
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}