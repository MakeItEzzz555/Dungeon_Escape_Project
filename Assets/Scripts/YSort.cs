using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int baseOrder = 1000;
    public int offset = 0;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Use the bottom of the sprite's boundary for sorting instead of the pivot/transform position
        float bottomY = sr.bounds.min.y;
        int newOrder = baseOrder - Mathf.RoundToInt(bottomY * 100) + offset;
        
        sr.sortingOrder = newOrder;

        // Debug line to see values in the console - you can remove this after fixing!
        // Debug.Log($"{gameObject.name}: Y={bottomY:F2}, Order={newOrder}");
    }
}