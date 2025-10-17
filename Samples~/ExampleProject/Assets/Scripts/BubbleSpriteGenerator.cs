using UnityEngine;

namespace BubbleShooter
{
    public static class BubbleSpriteGenerator
    {
        public static Sprite CreateCircleSprite(float radius, Color color)
        {
            // Create a texture
            var size = Mathf.CeilToInt(radius * 2 * 32); // 32 pixels per unit
            var texture = new Texture2D(size, size);

            // Create a circle
            var center = new Vector2(size / 2f, size / 2f);
            var radiusSquared = (size / 2f) * (size / 2f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var pixel = new Vector2(x, y);
                    var distance = Vector2.Distance(pixel, center);

                    if (distance <= size / 2f)
                    {
                        // Create gradient effect
                        var alpha = 1f - (distance / (size / 2f)) * 0.3f;
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            // Create sprite from texture
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        public static void ApplyCircleSprite(SpriteRenderer renderer, float radius, Color color)
        {
            var sprite = CreateCircleSprite(radius, color);
            renderer.sprite = sprite;
        }
    }
}