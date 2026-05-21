using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Code_Game.Scripts.UI;

public static class UIUtils
{
    public const float DefaultSpaceWidth = 8f;

    /// <summary>
    /// Draws a string while manually injecting spaces to compensate for fonts with zero-width spaces.
    /// </summary>
    public static void DrawStringWithManualSpaces(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, float scale, float manualSpaceWidth = DefaultSpaceWidth)
    {
        if (string.IsNullOrEmpty(text)) return;

        float scaledSpace = manualSpaceWidth * scale;
        Vector2 currentPos = pos;
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                sb.DrawString(font, words[i], currentPos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                currentPos.X += font.MeasureString(words[i]).X * scale;
            }
            
            if (i < words.Length - 1)
            {
                currentPos.X += scaledSpace;
            }
        }
    }

    /// <summary>
    /// Calculates the actual width of a string rendered with manual spaces.
    /// </summary>
    public static float CalculateTextWidth(SpriteFont font, string text, float scale, float manualSpaceWidth = DefaultSpaceWidth)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        float scaledSpace = manualSpaceWidth * scale;
        float totalWidth = 0;
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                totalWidth += font.MeasureString(words[i]).X * scale;
            }
            
            if (i < words.Length - 1)
            {
                totalWidth += scaledSpace;
            }
        }

        // Handle trailing spaces
        if (text.EndsWith(" "))
        {
            totalWidth += scaledSpace;
        }

        return totalWidth;
    }

    /// <summary>
    /// Draws a texture using 9-slice scaling.
    /// </summary>
    public static void Draw9Slice(SpriteBatch sb, Texture2D texture, Rectangle destRect, int edgeSize, Color color)
    {
        int w = texture.Width;
        int h = texture.Height;

        // Source rectangles
        Rectangle[] sources = new Rectangle[9];
        sources[0] = new Rectangle(0, 0, edgeSize, edgeSize); // Top-Left
        sources[1] = new Rectangle(edgeSize, 0, w - edgeSize * 2, edgeSize); // Top-Center
        sources[2] = new Rectangle(w - edgeSize, 0, edgeSize, edgeSize); // Top-Right
        sources[3] = new Rectangle(0, edgeSize, edgeSize, h - edgeSize * 2); // Middle-Left
        sources[4] = new Rectangle(edgeSize, edgeSize, w - edgeSize * 2, h - edgeSize * 2); // Middle-Center
        sources[5] = new Rectangle(w - edgeSize, edgeSize, edgeSize, h - edgeSize * 2); // Middle-Right
        sources[6] = new Rectangle(0, h - edgeSize, edgeSize, edgeSize); // Bottom-Left
        sources[7] = new Rectangle(edgeSize, h - edgeSize, w - edgeSize * 2, edgeSize); // Bottom-Center
        sources[8] = new Rectangle(w - edgeSize, h - edgeSize, edgeSize, edgeSize); // Bottom-Right

        // Destination rectangles
        Rectangle[] destinations = new Rectangle[9];
        destinations[0] = new Rectangle(destRect.X, destRect.Y, edgeSize, edgeSize);
        destinations[1] = new Rectangle(destRect.X + edgeSize, destRect.Y, destRect.Width - edgeSize * 2, edgeSize);
        destinations[2] = new Rectangle(destRect.Right - edgeSize, destRect.Y, edgeSize, edgeSize);
        destinations[3] = new Rectangle(destRect.X, destRect.Y + edgeSize, edgeSize, destRect.Height - edgeSize * 2);
        destinations[4] = new Rectangle(destRect.X + edgeSize, destRect.Y + edgeSize, destRect.Width - edgeSize * 2, destRect.Height - edgeSize * 2);
        destinations[5] = new Rectangle(destRect.Right - edgeSize, destRect.Y + edgeSize, edgeSize, destRect.Height - edgeSize * 2);
        destinations[6] = new Rectangle(destRect.X, destRect.Bottom - edgeSize, edgeSize, edgeSize);
        destinations[7] = new Rectangle(destRect.X + edgeSize, destRect.Bottom - edgeSize, destRect.Width - edgeSize * 2, edgeSize);
        destinations[8] = new Rectangle(destRect.Right - edgeSize, destRect.Bottom - edgeSize, edgeSize, edgeSize);

        for (int i = 0; i < 9; i++)
        {
            sb.Draw(texture, destinations[i], sources[i], color);
        }
    }

    public static void DrawRectangle(SpriteBatch sb, Texture2D pixel, Rectangle rect, Color color, int thickness = 1)
    {
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color); // Top
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color); // Bottom
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color); // Left
        sb.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color); // Right
    }
}
