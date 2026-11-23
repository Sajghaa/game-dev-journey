using System;
using System.Numerics;
using Raylib_cs;

Raylib.InitWindow(800, 600, "Stick Man Game");
Raylib.SetTargetFPS(60);

// 1. WORLD SETTINGS
float worldWidth = 3000f;
const float groundY    = 520f;
const float gravity    = 1500f;
const float jumpForce  = -650f;
const float moveSpeed  = 250f;

// Start position (for respawning)
Vector2 startPosition = new(100, groundY);
Vector2 position = startPosition;
float velocityY = 0f;
float walkTime  = 0f;
int deaths = 0;

Camera2D camera = new Camera2D();
camera.Offset = new Vector2(800 / 2, 600 / 2);
camera.Target = new Vector2(position.X, position.Y - 50);
camera.Rotation = 0f;
camera.Zoom = 1.0f;

// 2. PLATFORMS
Rectangle[] platforms = {
    new(120, 420, 140, 15),
    new(320, 340, 140, 15),
    new(540, 420, 140, 15),
    new(900, 380, 140, 15),   
    new(1150, 300, 140, 15),  
    new(1400, 420, 140, 15),  
    new(1650, 340, 140, 15),  
    new(1900, 420, 140, 15),  
    new(2200, 300, 140, 15),  
    new(2500, 400, 140, 15),  
};

// 3. NEW: SPIKES! (x, y, width, height)
Rectangle[] spikes = {
    new(700, groundY - 20, 100, 20),  // On the ground
    new(1420, 400, 100, 20),          // On a platform
    new(1950, groundY - 20, 200, 20), // A wider pit on the ground
    new(2520, 380, 100, 20)           // Near the end
};

while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();

    // ----- Horizontal movement -----
    float dx = 0f;
    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))  dx -= 1f;
    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) dx += 1f;
    position.X += dx * moveSpeed * dt;
    position.X = Math.Clamp(position.X, 20f, worldWidth - 20f);

    // ----- Check if on ground or a platform -----
    bool onGround = position.Y >= groundY - 1f;
    foreach (var p in platforms)
        if (Math.Abs(position.Y - p.Y) < 2f
            && position.X >= p.X - 15 && position.X <= p.X + p.Width + 15)
            onGround = true;

    // ----- Jump -----
    if (onGround && (Raylib.IsKeyPressed(KeyboardKey.Space)
                  || Raylib.IsKeyPressed(KeyboardKey.W)
                  || Raylib.IsKeyPressed(KeyboardKey.Up)))
    {
        velocityY = jumpForce;
        onGround = false;
    }

    // ----- Gravity + landing -----
    float prevY = position.Y;
    velocityY += gravity * dt;
    position.Y += velocityY * dt;

    if (velocityY >= 0f)
    {
        foreach (var p in platforms)
            if (position.X >= p.X - 15 && position.X <= p.X + p.Width + 15
                && prevY <= p.Y && position.Y >= p.Y)
            {
                position.Y = p.Y;
                velocityY = 0f;
            }
    }
    if (position.Y > groundY) { position.Y = groundY; velocityY = 0f; }

    // ----- NEW: SPIKE COLLISION -----
    // Create a rectangle around the stick man for hit detection
    Rectangle playerRect = new Rectangle(position.X - 15, position.Y - 90, 30, 90);
    
    foreach (var spike in spikes)
    {
        if (Raylib.CheckCollisionRecs(playerRect, spike))
        {
            // BAM! Hit a spike.
            position = startPosition; // Respawn at start
            velocityY = 0f;           // Stop falling
            deaths++;                 // Increase death count
            break;                    // Stop checking other spikes this frame
        }
    }

    // ----- Walk animation timer -----
    if (dx != 0f && (position.Y >= groundY || onGround)) walkTime += dt * 12f;
    else if (dx == 0f) walkTime = 0f;
    bool inAir = position.Y < groundY - 2f && !onGround;

    // ----- CAMERA UPDATE -----
    float camX = Math.Clamp(position.X, 400f, worldWidth - 400f);
    camera.Target = new Vector2(camX, position.Y - 100);

    // ----- Draw -----
    Raylib.BeginDrawing();
    Raylib.ClearBackground(new Color(135, 206, 235, 255));

    Raylib.BeginMode2D(camera);

    // Draw Ground
    Raylib.DrawRectangle(0, (int)groundY, (int)worldWidth, 80, new Color(34, 139, 34, 255));

    // Draw Platforms
    foreach (var p in platforms)
        Raylib.DrawRectangleRec(p, new Color(139, 69, 19, 255));

    // Draw Spikes (using a helper function at the bottom)
    foreach (var spike in spikes)
        DrawSpikes(spike);

    DrawStickMan(position, walkTime, inAir);

    Raylib.EndMode2D();

    // Draw UI
    Raylib.DrawText("A/D or arrows = move   Space/W = jump", 10, 10, 20, Color.Black);
    Raylib.DrawText($"Deaths: {deaths}", 10, 40, 20, Color.Red); // Show deaths!
    
    Raylib.EndDrawing();
}

Raylib.CloseWindow();

// ----- Helper: Draw Stick Man -----
void DrawStickMan(Vector2 feet, float walkTime, bool inAir)
{
    Color c = Color.Black;
    Vector2 head = new(feet.X, feet.Y - 90);
    Vector2 neck = new(feet.X, feet.Y - 70);
    Vector2 hip  = new(feet.X, feet.Y - 35);

    Raylib.DrawCircleV(head, 12, c);
    Raylib.DrawLineV(head, hip, c);

    if (inAir)
    {
        Raylib.DrawLineV(neck, new Vector2(neck.X - 25, neck.Y - 20), c);
        Raylib.DrawLineV(neck, new Vector2(neck.X + 25, neck.Y - 20), c);
        Raylib.DrawLineV(hip,  new Vector2(hip.X  - 18, feet.Y),     c);
        Raylib.DrawLineV(hip,  new Vector2(hip.X  + 18, feet.Y),     c);
    }
    else if (walkTime > 0f)
    {
        float s = (float)Math.Sin(walkTime) * 20f;
        Raylib.DrawLineV(neck, new Vector2(neck.X + s, neck.Y + 30), c);
        Raylib.DrawLineV(neck, new Vector2(neck.X - s, neck.Y + 30), c);
        Raylib.DrawLineV(hip,  new Vector2(hip.X  + s, feet.Y),      c);
        Raylib.DrawLineV(hip,  new Vector2(hip.X  - s, feet.Y),      c);
    }
    else
    {
        Raylib.DrawLineV(neck, new Vector2(neck.X - 15, neck.Y + 30), c);
        Raylib.DrawLineV(neck, new Vector2(neck.X + 15, neck.Y + 30), c);
        Raylib.DrawLineV(hip,  new Vector2(hip.X  - 10, feet.Y),      c);
        Raylib.DrawLineV(hip,  new Vector2(hip.X  + 10, feet.Y),      c);
    }
}

// ----- Helper: Draw Spikes -----
void DrawSpikes(Rectangle area)
{
    // Figure out how many triangles fit in the width
    int numSpikes = (int)(area.Width / 20); 
    float spikeWidth = area.Width / numSpikes;

    for (int i = 0; i < numSpikes; i++)
    {
        Vector2 bottomLeft = new(area.X + (i * spikeWidth), area.Y + area.Height);
        Vector2 topMiddle  = new(area.X + (i * spikeWidth) + (spikeWidth / 2), area.Y);
        Vector2 bottomRight= new(area.X + ((i + 1) * spikeWidth), area.Y + area.Height);

        // Draw the red triangle
        Raylib.DrawTriangle(bottomLeft, topMiddle, bottomRight, Color.Red);
        Raylib.DrawTriangle(bottomRight, topMiddle, bottomLeft, Color.Red); // Draw twice to fix Raylib line gaps
    }
}