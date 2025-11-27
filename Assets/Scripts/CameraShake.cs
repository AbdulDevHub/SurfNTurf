using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Camera cam;
    private Vector3 shakeStartPos;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;

        // Store the camera's position at the moment shake begins
        shakeStartPos = transform.localPosition;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            transform.localPosition = new Vector3(
                shakeStartPos.x + x,
                shakeStartPos.y + y,
                shakeStartPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset only to the pre-shake position (not game start)
        transform.localPosition = shakeStartPos;
    }

    // Camera now focuses on dam but does NOT return afterward
    public IEnumerator FocusOnPoint(Vector3 target, float zoomHeight, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(target.x, zoomHeight, target.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }
}
