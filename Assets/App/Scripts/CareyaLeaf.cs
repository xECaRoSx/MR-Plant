using UnityEngine;

public class SummerLeafColorEffect : MonoBehaviour
{
    [Header("Target Material to Change")]
    public Material targetMaterial;

    [Header("Leaf Color Settings")]
    public Color startColor = new Color(0.572f, 0.722f, 0.212f); // Green 92B836
    public Color endColor = new Color(0.866f, 0.231f, 0.0f);     // Red DD3B00

    [Header("Transition Duration")]
    public float duration = 5f;

    private float timer = 0f;
    private bool fadingForward = false;

    private void OnEnable()
    {
        timer = 0f;
        fadingForward = true;

        if (targetMaterial != null)
            targetMaterial.color = startColor;
    }

    private void Update()
    {
        if (!fadingForward || targetMaterial == null)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);
        targetMaterial.color = Color.Lerp(startColor, endColor, t);
    }

    private void OnDisable()
    {
        // When turning off, immediately revert color
        if (targetMaterial != null)
            targetMaterial.color = startColor;

        fadingForward = false;
        timer = 0f;
    }
}
