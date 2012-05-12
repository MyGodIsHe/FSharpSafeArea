module SafeArea.AlignedSpriteBatch

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics



/// <summary>
/// Flags enum defines the various ways a text string
/// can be aligned relative to its specified position.
/// </summary>
type Alignment = 
    // Horizontal alignment options.
    | Left = 0
    | Right = 1
    | HorizontalCenter = 2

    // Vertical alignment options.
    | Top = 0
    | Bottom = 4
    | VerticalCenter = 8

    // Combined vertical + horizontal alignment options.
    | TopLeft = 0
    | TopRight = 1
    | TopCenter = 2

    | BottomLeft = 4
    | BottomRight = 5
    | BottomCenter = 6

    | CenterLeft = 8
    | CenterRight = 9
    | Center = 10


/// <summary>
/// This class derives from the built in SpriteBatch, adding new
/// logic for aligning text strings in more varied ways than just
/// the default top left alignment.
/// </summary>
type AlignedSpriteBatch(graphicsDevice : GraphicsDevice) =
    inherit SpriteBatch(graphicsDevice)


    /// <summary>
    /// Draws a text string with the specified alignment.
    /// </summary>
    member public this.DrawString(spriteFont : SpriteFont, text : string, position : Vector2, color : Color, alignment : Alignment) =
        let mutable position = position
        // Compute horizontal alignment.
        if int (alignment &&& Alignment.Right) <> 0
        then position.X <- position.X - spriteFont.MeasureString(text).X
        elif int (alignment &&& Alignment.HorizontalCenter) <> 0
        then position.X <- position.X - spriteFont.MeasureString(text).X / 2.0f

        // Compute vertical alignment.
        if int (alignment &&& Alignment.Bottom) <> 0
        then position.Y <- position.Y - float32 spriteFont.LineSpacing
        elif int (alignment &&& Alignment.VerticalCenter) <> 0
        then position.Y <- position.Y - float32 spriteFont.LineSpacing / 2.0f

        // Draw the string.
        this.DrawString(spriteFont, text, position, color)
