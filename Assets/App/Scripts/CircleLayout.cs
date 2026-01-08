using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CircleLayoutCenter : MonoBehaviour
{
    public enum LayoutMode
    {
        FullCircle,
        HalfCircle
    }

    [Header("Layout Settings")]
    [Range(0.1f, 100f)]
    public float radius = 5f;

    [Tooltip("Start Angle (0 = +X, 90 = +Z)")]
    public float startAngleDeg = 0f;

    public LayoutMode layoutMode = LayoutMode.FullCircle;

    [Min(1)]
    public int rows = 1;

    public float rowSpacing = 1.5f;

    [Header("Zigzag (Fishbone)")]
    public bool useZigzag = false;

    public bool lookAtCenter = true;
    public bool lookOutward = false;

    [Header("Target Objects")]
    public List<Transform> children = new List<Transform>();

    [Header("Plane")]
    public bool useXZPlane = true;

    public void Arrange()
    {
        if (children == null || children.Count == 0 || rows <= 0)
            return;

        int total = children.Count;
        int basePerRow = total / rows;
        int remainder = total % rows;

        float angleRange = (layoutMode == LayoutMode.FullCircle) ? 360f : 180f;

        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            int countInThisRow = basePerRow;
            if (row >= rows - remainder)
                countInThisRow += 1;

            float currentRadius = radius + row * rowSpacing;

            if (countInThisRow <= 0)
                break;

            float step = (countInThisRow > 1)
                ? angleRange / (layoutMode == LayoutMode.FullCircle ? countInThisRow : countInThisRow - 1)
                : 0f;

            float zigzagOffset = 0f;
            if (useZigzag && row % 2 == 1)
            {
                zigzagOffset = step * 0.5f;
            }

            for (int i = 0; i < countInThisRow; i++)
            {
                if (index >= total) return;
                if (children[index] == null)
                {
                    index++;
                    continue;
                }

                float angle = startAngleDeg + zigzagOffset + step * i;
                float rad = angle * Mathf.Deg2Rad;

                float x = currentRadius * Mathf.Cos(rad);
                float y = currentRadius * Mathf.Sin(rad);

                Vector3 localPos = useXZPlane
                    ? new Vector3(x, 0f, y)
                    : new Vector3(x, y, 0f);

                children[index].localPosition = localPos;

                if (lookAtCenter)
                {
                    Vector3 dir = -localPos.normalized;
                    if (dir != Vector3.zero)
                        children[index].localRotation = Quaternion.LookRotation(dir);
                }
                else if (lookOutward)
                {
                    Vector3 dir = localPos.normalized;
                    if (dir != Vector3.zero)
                        children[index].localRotation = Quaternion.LookRotation(dir);
                }

                index++;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Arrange();
    }
#endif
}
