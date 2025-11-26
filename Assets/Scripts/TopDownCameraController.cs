using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minHeight = 5f;
    public float maxHeight = 40f;
    public float keyZoomMultiplier = 0.3f;

    [Header("Map Bounds")]
    public Transform map;  // Assign "Map Sketch" here

    private Camera cam;
    private Bounds mapBounds;

    private void Start()
    {
        cam = GetComponent<Camera>();

        Collider col = map.GetComponent<Collider>();
        if (col != null)
        {
            mapBounds = col.bounds;
            return;
        }

        Collider2D col2D = map.GetComponent<Collider2D>();
        if (col2D != null)
        {
            mapBounds = col2D.bounds;
            return;
        }

        Debug.LogError("Map has no Renderer or Collider for bounds calculation!");
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        ClampToMap();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0f, v);
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.Q))
            scroll += keyZoomMultiplier * Time.deltaTime;

        if (Input.GetKey(KeyCode.E))
            scroll -= keyZoomMultiplier * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y -= scroll * zoomSpeed;
        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);

        transform.position = pos;
    }

    private void ClampToMap()
    {
        Vector3 pos = transform.position;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        // Clamp X and Z so the camera view doesn't leave the map bounds
        pos.x = Mathf.Clamp(
            pos.x,
            mapBounds.min.x + camHalfWidth,
            mapBounds.max.x - camHalfWidth
        );

        pos.z = Mathf.Clamp(
            pos.z,
            mapBounds.min.z + camHalfHeight,
            mapBounds.max.z - camHalfHeight
        );

        transform.position = pos;
    }
}
