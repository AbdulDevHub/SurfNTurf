using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minHeight = 5f;
    public float maxHeight = 40f;

    [Tooltip("Multiplier to control Q/E zoom speed. Lower = slower.")]
    public float keyZoomMultiplier = 0.3f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");  // A/D
        float v = Input.GetAxisRaw("Vertical");    // W/S

        Vector3 move = new Vector3(h, 0f, v);
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        // Mouse scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Q/E slower zoom
        if (Input.GetKey(KeyCode.Q))
            scroll += keyZoomMultiplier * Time.deltaTime;

        if (Input.GetKey(KeyCode.E))
            scroll -= keyZoomMultiplier * Time.deltaTime;

        // Apply zoom
        Vector3 pos = transform.position;
        pos.y -= scroll * zoomSpeed;
        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);

        transform.position = pos;
    }
}
