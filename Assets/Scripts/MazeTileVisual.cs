using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MazeTileVisual : MonoBehaviour
{
    private static Color markedColour = new Color(1f, 0.4f, 0.4f);
    private static Color unmarkedColour = Color.white;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ChangeSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
    }

    public void SetColourMarked()
    {
        spriteRenderer.color = markedColour;
    }

    public void SetColourUnmarked()
    {
        spriteRenderer.color = unmarkedColour;
    }
}
