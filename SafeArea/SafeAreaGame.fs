module SafeArea.SafeAreaGame

open System.IO
open System.Reflection
open System.Resources
open System.Drawing

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Content

open SafeArea.AlignedSpriteBatch


/// <summary>
/// Sample showing how to handle television safe areas in an XNA Framework game.
/// </summary>
type SafeAreaGame() as this =
    inherit Microsoft.Xna.Framework.Game()

    let ScreenWidth = 1280
    let ScreenHeight = 720

    let mutable resourceManager : ResourceManager = null
    let mutable graphics : GraphicsDeviceManager = null
    let mutable spriteBatch : AlignedSpriteBatch = Unchecked.defaultof<_>

    let mutable catTexture : Texture2D = null
    let mutable backgroundTexture : Texture2D = null

    let mutable catPosition : Vector2 = Vector2.Zero
    let mutable catVelocity : Vector2 = Vector2.Zero

    let mutable cameraPosition : Vector2 = Vector2.Zero

    let mutable currentKeyboardState : KeyboardState = Keyboard.GetState()
    let mutable currentGamePadState : GamePadState = GamePad.GetState(PlayerIndex.One)

    let mutable previousKeyboardState : KeyboardState = Keyboard.GetState()
    let mutable previousGamePadState : GamePadState = GamePad.GetState(PlayerIndex.One)

    do
        resourceManager <- new ResourceManager("Content", Assembly.GetExecutingAssembly())
        //this.Content <- new ResourceContentManager(this.Services, resourceManager)
        //this.Content.RootDirectory <- "Content"

        graphics <- new GraphicsDeviceManager(this)

        graphics.PreferredBackBufferWidth <- ScreenWidth
        graphics.PreferredBackBufferHeight <- ScreenHeight


    /// <summary>
    /// Load your graphics content.
    /// </summary>
    override this.LoadContent() =
        let LoadPng name =
            let image = resourceManager.GetObject(name) :?> Image
            let stream = new MemoryStream()
            image.Save(stream, Imaging.ImageFormat.Png)
            Texture2D.FromStream(this.GraphicsDevice, stream)

        spriteBatch <- new AlignedSpriteBatch(this.GraphicsDevice)

        catTexture <- LoadPng "Cat"
        backgroundTexture <- LoadPng "Background"


    /// <summary>
    /// Allows the game to run logic.
    /// </summary>
    override this.Update(gameTime : GameTime) =
        this.HandleInput()

        this.UpdateCat()
        this.UpdateCamera()

        base.Update(gameTime)


    /// <summary>
    /// Moves the cat sprite around the screen.
    /// </summary>
    member this.UpdateCat() =
        let speedOfCat = 0.75f
        let catFriction = 0.9f

        // Apply gamepad input.
        let flipY = new Vector2(1.0f, -1.0f)

        catVelocity <- catVelocity + currentGamePadState.ThumbSticks.Left * flipY * speedOfCat

        // Apply keyboard input.
        if currentKeyboardState.IsKeyDown(Keys.Left)
        then catVelocity.X <- catVelocity.X - speedOfCat

        if currentKeyboardState.IsKeyDown(Keys.Right)
        then catVelocity.X <- catVelocity.X + speedOfCat

        if currentKeyboardState.IsKeyDown(Keys.Up)
        then catVelocity.Y <- catVelocity.Y - speedOfCat

        if currentKeyboardState.IsKeyDown(Keys.Down)
        then catVelocity.Y <- catVelocity.Y + speedOfCat

        // Apply velocity and friction.
        catPosition <- catPosition + catVelocity
        catVelocity <- catVelocity * catFriction


    /// <summary>
    /// Updates the camera position, scrolling the
    /// screen if the cat gets too close to the edge.
    /// </summary>
    member this.UpdateCamera() =
        // How far away from the camera should we allow the cat
        // to move before we scroll the camera to follow it?
        let mutable maxScroll = Vector2(float32 ScreenWidth, float32 ScreenHeight) / 2.0f

        // Apply a safe area to prevent the cat getting too close to the edge
        // of the screen. Note that this is even more restrictive than the 80%
        // safe area used for the overlays, because we want to start scrolling
        // even before the cat gets right up to the edge of the legal area.
        let catSafeArea = 0.7f
        maxScroll <- maxScroll * catSafeArea

        // Adjust for the size of the cat sprite, so we will start
        // scrolling based on the edge rather than center of the cat.
        //maxScroll -= new Vector2(catTexture.Width, catTexture.Height) / 2

        // Make sure the camera stays within the desired distance of the cat.
        let min = catPosition - maxScroll
        let max = catPosition + maxScroll

        cameraPosition.X <- MathHelper.Clamp(cameraPosition.X, min.X, max.X)
        cameraPosition.Y <- MathHelper.Clamp(cameraPosition.Y, min.Y, max.Y)


    override this.Draw(gameTime : GameTime) =
        this.GraphicsDevice.Clear(Color.Black)

        // Work out how far to scroll based on the current camera position.
        let screenCenter = Vector2(float32 ScreenWidth, float32 ScreenHeight) / 2.0f
        let scrollOffset = screenCenter - cameraPosition

        // Draw the background, cat, and text overlays.
        spriteBatch.Begin()

        this.DrawBackground(scrollOffset)
        this.DrawCat(scrollOffset)

        spriteBatch.End()

        base.Draw(gameTime)


    member this.DrawBackground(scrollOffset : Vector2) =
        // Work out the position of the top left visible tile.
        let mutable tileX = (int)scrollOffset.X % backgroundTexture.Width
        let mutable tileY = (int)scrollOffset.Y % backgroundTexture.Height

        if tileX > 0
        then tileX <- tileX - backgroundTexture.Width

        if tileY > 0
        then tileY <- tileY - backgroundTexture.Height

        spriteBatch.Draw(backgroundTexture, Vector2(float32 tileX, float32 tileY), Color.White)

        // Draw however many repeating tiles are needed to cover the screen.
        for x in [tileX .. backgroundTexture.Width .. ScreenWidth] do
            for y in [tileY .. backgroundTexture.Height .. ScreenHeight] do
                spriteBatch.Draw(backgroundTexture, Vector2(float32 x, float32 y), Color.White)


    member this.DrawCat(scrollOffset : Vector2) =
        let catCenter = Vector2(float32 catTexture.Width, float32 catTexture.Height) / 2.0f
        let position = catPosition - catCenter + scrollOffset
        spriteBatch.Draw(catTexture, position, Color.White)


    member private this.HandleInput() =
        previousKeyboardState <- currentKeyboardState
        previousGamePadState <- currentGamePadState

        currentKeyboardState <- Keyboard.GetState()
        currentGamePadState <- GamePad.GetState(PlayerIndex.One)

        // Check for exit.
        if currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.IsButtonDown(Buttons.Back)
        then this.Exit()


/// <summary>
/// The main entry point for the application.
/// </summary>
[<EntryPoint>]
let main args =
    (new SafeAreaGame()).Run()
    0