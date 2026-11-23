using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

const int screenWidth = 1000;
const int screenHeight = 650;
const float groundY = 540f;
const float gravity = 1500f;
const float jumpForce = -650f;
const float moveSpeed = 280f;

Raylib.InitWindow(screenWidth, screenHeight, "Stick Man Adventure");
Raylib.SetTargetFPS(60);

int deaths = 0;
int currentLevelIndex = 0;
bool hasWon = false;
bool isPaused = false;

List<Level> levels = new()
{
    new Level
    {
        WorldWidth = 3200f,
        GoalX = 3000f,
        StartPos = new Vector2(100, groundY),
        Platforms = new Rectangle[]
        {
            new(300, 440, 150, 22),
            new(560, 350, 150, 22),
            new(850, 440, 150, 22),
            new(1350, 420, 170, 22),
            new(1640, 330, 170, 22),
            new(1950, 420, 170, 22),
            new(2350, 360, 180, 22)
        },
        Spikes = new Rectangle[]
        {
            new(1050, groundY - 28, 130, 28),
            new(1780, groundY - 28, 120, 28),
            new(2150, groundY - 28, 140, 28)
        },
        Enemies = new List<Enemy>
        {
            new(650, 950, groundY - 22),
            new(1450, 1700, groundY - 22),
            new(2450, 2750, groundY - 22)
        }
    },
    new Level
    {
        WorldWidth = 3500f,
        GoalX = 3300f,
        StartPos = new Vector2(100, groundY),
        Platforms = new Rectangle[]
        {
            new(250, 450, 130, 22),
            new(470, 360, 130, 22),
            new(700, 270, 130, 22),
            new(1100, 430, 180, 22),
            new(1430, 340, 150, 22),
            new(1700, 250, 150, 22),
            new(2150, 420, 180, 22),
            new(2500, 330, 160, 22),
            new(2800, 420, 160, 22)
        },
        Spikes = new Rectangle[]
        {
            new(870, groundY - 28, 180, 28),
            new(1870, groundY - 28, 180, 28),
            new(2350, groundY - 28, 130, 28),
            new(3000, groundY - 28, 100, 28)
        },
        Enemies = new List<Enemy>
        {
            new(1150, 1350, groundY - 22),
            new(2200, 2450, groundY - 22),
            new(2850, 3100, groundY - 22)
        }
    }
};

Level currentLevel = levels[currentLevelIndex];
Vector2 position = currentLevel.StartPos; // Player position is the centre of the feet.
float velocityY = 0f;
float walkTime = 0f;
float jumpSquash = 0f;
bool isFacingRight = true;
bool wasOnGround = true;

Camera2D camera = new()
{
    Offset = new Vector2(screenWidth / 2f, screenHeight / 2f),
    Target = new Vector2(screenWidth / 2f, screenHeight / 2f),
    Rotation = 0f,
    Zoom = 1f
};

while (!Raylib.WindowShouldClose())
{
    float dt = Math.Min(Raylib.GetFrameTime(), 0.033f);

    if (Raylib.IsKeyPressed(KeyboardKey.P) || Raylib.IsKeyPressed(KeyboardKey.Escape))
        isPaused = !isPaused;

    if (hasWon)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            currentLevelIndex = (currentLevelIndex + 1) % levels.Count;
            currentLevel = levels[currentLevelIndex];
            position = currentLevel.StartPos;
            velocityY = 0f;
            hasWon = false;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
        }
    }
    else if (!isPaused)
    {
        // Player movement
        float dx = 0f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
        {
            dx -= 1f;
            isFacingRight = false;
        }
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
        {
            dx += 1f;
            isFacingRight = true;
        }

        position.X += dx * moveSpeed * dt;
        position.X = Math.Clamp(position.X, 20f, currentLevel.WorldWidth - 20f);

        // Work out whether the player is standing on the ground or a platform.
        bool onGround = position.Y >= groundY - 1f;
        if (onGround)
        {
            position.Y = groundY;
            velocityY = Math.Max(0f, velocityY);
        }

        Rectangle playerFeet = new(position.X - 13f, position.Y - 7f, 26f, 14f);
        foreach (Rectangle platform in currentLevel.Platforms)
        {
            Rectangle platformTop = new(platform.X, platform.Y - 4f, platform.Width, 8f);
            if (Raylib.CheckCollisionRecs(playerFeet, platformTop))
                onGround = true;
        }

        if (onGround && (Raylib.IsKeyPressed(KeyboardKey.Space) ||
                         Raylib.IsKeyPressed(KeyboardKey.W) ||
                         Raylib.IsKeyPressed(KeyboardKey.Up)))
        {
            velocityY = jumpForce;
            onGround = false;
        }

        float previousY = position.Y;
        velocityY += gravity * dt;
        position.Y += velocityY * dt;

        // Land on platforms only while falling.
        if (velocityY >= 0f)
        {
            foreach (Rectangle platform in currentLevel.Platforms)
            {
                bool overlapsX = position.X + 13f > platform.X &&
                                 position.X - 13f < platform.X + platform.Width;
                bool crossedTop = previousY <= platform.Y && position.Y >= platform.Y;
                if (overlapsX && crossedTop)
                {
                    position.Y = platform.Y;
                    velocityY = 0f;
                    onGround = true;
                }
            }
        }

        if (position.Y >= groundY)
        {
            position.Y = groundY;
            velocityY = 0f;
            onGround = true;
        }

        bool inAir = !onGround;
        if (dx != 0f && !inAir) walkTime += dt * 11f;
        jumpSquash += ((inAir ? 0.12f : 0f) - jumpSquash) * Math.Min(1f, dt * 8f);

        // Collision body.
        Rectangle playerRect = new(position.X - 14f, position.Y - 72f, 28f, 68f);
        bool hitHazard = false;

        foreach (Rectangle spike in currentLevel.Spikes)
        {
            Rectangle spikeHitbox = new(spike.X + 5f, spike.Y + 8f, spike.Width - 10f, spike.Height - 8f);
            if (Raylib.CheckCollisionRecs(playerRect, spikeHitbox))
                hitHazard = true;
        }

        foreach (Enemy enemy in currentLevel.Enemies)
        {
            if (enemy.IsSquashed) continue;

            Rectangle enemyRect = new(enemy.Position.X - 19f, enemy.Position.Y - 34f, 38f, 34f);
            if (Raylib.CheckCollisionRecs(playerRect, enemyRect))
            {
                bool stomping = velocityY > 0f && previousY - 72f <= enemyRect.Y + 12f;
                if (stomping)
                {
                    enemy.IsSquashed = true;
                    velocityY = -440f;
                }
                else
                {
                    hitHazard = true;
                }
            }
        }

        if (hitHazard)
        {
            position = currentLevel.StartPos;
            velocityY = 0f;
            deaths++;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
        }

        foreach (Enemy enemy in currentLevel.Enemies)
            enemy.Update(dt);

        if (position.X >= currentLevel.GoalX - 28f && position.Y >= groundY - 2f)
            hasWon = true;

        wasOnGround = onGround;
    }

    // Smooth camera follow.
    float cameraX = Math.Clamp(position.X, screenWidth / 2f, currentLevel.WorldWidth - screenWidth / 2f);
    camera.Target = Vector2.Lerp(camera.Target, new Vector2(cameraX, position.Y - 100f), 0.10f);

    Raylib.BeginDrawing();
    DrawSkyAndHud(currentLevelIndex + 1, deaths, currentLevel, position.X);

    Raylib.BeginMode2D(camera);

    DrawCartoonBackground(currentLevel.WorldWidth);
    DrawWorldGround(currentLevel.WorldWidth);

    foreach (Rectangle platform in currentLevel.Platforms)
        DrawGrassPlatform(platform);

    foreach (Rectangle spike in currentLevel.Spikes)
        DrawCartoonSpikes(spike);

    foreach (Enemy enemy in currentLevel.Enemies)
        DrawEnemy(enemy);

    DrawGoal(currentLevel.GoalX);
    DrawStickMan(position, walkTime, velocityY < 0f, isFacingRight, jumpSquash);

    Raylib.EndMode2D();

    if (isPaused && !hasWon)
        DrawOverlay("PAUSED", "Press P or ESC to continue");
    if (hasWon)
        DrawOverlay($"LEVEL {currentLevelIndex + 1} COMPLETE!", "Press R for the next adventure");

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

// ---------------- DRAWING HELPERS ----------------

void DrawSkyAndHud(int levelNumber, int deathCount, Level level, float playerX)
{
    Raylib.ClearBackground(new Color(116, 205, 255, 255));

    // Soft sun and clouds are drawn in screen space.
    Raylib.DrawCircle(870, 95, 42, new Color(255, 226, 112, 255));
    DrawCloud(140, 105, 1f);
    DrawCloud(440, 75, 0.75f);
    DrawCloud(700, 165, 0.9f);

    Raylib.DrawRectangle(0, 0, screenWidth, 12, new Color(255, 255, 255, 70));

    // HUD panel
    Raylib.DrawRectangle(18, 18, 245, 82, new Color(25, 55, 82, 220));
    Raylib.DrawRectangle(18, 18, 245, 7, new Color(255, 210, 75, 255));
    Raylib.DrawText($"LEVEL {levelNumber}", 34, 34, 23, Color.White);
    Raylib.DrawText($"FALLS: {deathCount}", 35, 66, 18, new Color(255, 180, 180, 255));

    // Progress bar
    float progress = Math.Clamp(playerX / level.GoalX, 0f, 1f);
    Raylib.DrawRectangle(700, 32, 260, 18, new Color(25, 55, 82, 210));
    Raylib.DrawRectangle(704, 36, (int)(252 * progress), 10, new Color(255, 210, 75, 255));
    Raylib.DrawText("ADVENTURE PROGRESS", 700, 12, 15, Color.White);
    Raylib.DrawText("A/D or arrows: move     SPACE: jump     P: pause", 310, 615, 17, Color.White);
}

void DrawCloud(float x, float y, float scale)
{
    Color cloud = new(255, 255, 255, 235);
    Raylib.DrawCircleV(new Vector2(x, y), 23 * scale, cloud);
    Raylib.DrawCircleV(new Vector2(x + 25 * scale, y - 12 * scale), 30 * scale, cloud);
    Raylib.DrawCircleV(new Vector2(x + 55 * scale, y), 23 * scale, cloud);
    Raylib.DrawRectangle((int)(x - 2 * scale), (int)y, (int)(60 * scale), (int)(19 * scale), cloud);
}

void DrawCartoonBackground(float worldWidth)
{
    // Distant rolling hills.
    for (int i = 0; i < (int)(worldWidth / 420f) + 2; i++)
    {
        float x = i * 420f;
        Raylib.DrawCircleV(new Vector2(x + 120, groundY - 55), 145, new Color(111, 190, 116, 255));
        Raylib.DrawCircleV(new Vector2(x + 310, groundY - 45), 125, new Color(92, 174, 105, 255));
    }

    // Little decorative flowers along the ground.
    for (int i = 0; i < (int)(worldWidth / 170f); i++)
    {
        float x = i * 170f + 55f;
        Raylib.DrawLineEx(new Vector2(x, groundY - 5), new Vector2(x, groundY - 23), 3, new Color(38, 130, 65, 255));
        Raylib.DrawCircle((int)x, (int)(groundY - 25), 5, new Color(255, 220, 80, 255));
        Raylib.DrawCircle((int)(x - 5), (int)(groundY - 28), 3, Color.White);
        Raylib.DrawCircle((int)(x + 5), (int)(groundY - 28), 3, Color.White);
    }
}

void DrawWorldGround(float worldWidth)
{
    Raylib.DrawRectangle(0, (int)groundY, (int)worldWidth, 120, new Color(133, 83, 48, 255));
    Raylib.DrawRectangle(0, (int)groundY, (int)worldWidth, 15, new Color(65, 177, 75, 255));
    Raylib.DrawRectangle(0, (int)groundY + 15, (int)worldWidth, 5, new Color(45, 143, 61, 255));

    for (int i = 0; i < (int)(worldWidth / 65f); i++)
    {
        float x = i * 65f + 18f;
        Raylib.DrawCircle((int)x, (int)(groundY + 43 + (i % 3) * 13), 3, new Color(101, 61, 39, 255));
    }
}

void DrawGrassPlatform(Rectangle platform)
{
    Raylib.DrawRectangleRec(platform, new Color(151, 91, 51, 255));
    Raylib.DrawRectangle((int)platform.X, (int)platform.Y, (int)platform.Width, 9, new Color(66, 181, 76, 255));
    Raylib.DrawRectangle((int)platform.X, (int)platform.Y + 9, (int)platform.Width, 4, new Color(45, 145, 59, 255));

    for (int i = 0; i < (int)(platform.Width / 24f); i++)
    {
        float x = platform.X + 12 + i * 24;
        Raylib.DrawLineEx(new Vector2(x, platform.Y + 2), new Vector2(x - 3, platform.Y - 5), 2, new Color(39, 145, 59, 255));
    }

    for (int i = 0; i < (int)(platform.Width / 42f); i++)
        Raylib.DrawCircle((int)(platform.X + 18 + i * 42), (int)(platform.Y + 17), 2, new Color(105, 62, 38, 255));
}

void DrawCartoonSpikes(Rectangle area)
{
    int count = Math.Max(1, (int)(area.Width / 24f));
    float width = area.Width / count;

    for (int i = 0; i < count; i++)
    {
        Vector2 left = new(area.X + i * width, area.Y + area.Height);
        Vector2 top = new(area.X + i * width + width / 2f, area.Y + 2);
        Vector2 right = new(area.X + (i + 1) * width, area.Y + area.Height);
        Raylib.DrawTriangle(left, top, right, new Color(239, 76, 75, 255));
        Raylib.DrawLineEx(left, top, 2, new Color(255, 160, 145, 255));
        Raylib.DrawLineEx(top, right, 2, new Color(166, 43, 48, 255));
    }
    Raylib.DrawRectangle((int)area.X, (int)(area.Y + area.Height - 4), (int)area.Width, 4, new Color(125, 54, 52, 255));
}

void DrawEnemy(Enemy enemy)
{
    float x = enemy.Position.X;
    float y = enemy.Position.Y;

    if (enemy.IsSquashed)
    {
        Raylib.DrawEllipse((int)x, (int)(y - 4), 22, 7, new Color(151, 55, 68, 255));
        return;
    }

    float bob = (float)Math.Sin(enemy.BobTime * 8f) * 2f;
    Raylib.DrawEllipse((int)x, (int)(y - 2), 23, 6, new Color(42, 75, 54, 90));
    Raylib.DrawCircleV(new Vector2(x, y - 19 + bob), 19, new Color(218, 75, 91, 255));
    Raylib.DrawCircleV(new Vector2(x - 6, y - 23 + bob), 5, Color.White);
    Raylib.DrawCircleV(new Vector2(x + 6, y - 23 + bob), 5, Color.White);

    float eyeDirection = enemy.Direction >= 0 ? 1f : -1f;
    Raylib.DrawCircleV(new Vector2(x - 6 + eyeDirection * 2, y - 23 + bob), 2.4f, Color.Black);
    Raylib.DrawCircleV(new Vector2(x + 6 + eyeDirection * 2, y - 23 + bob), 2.4f, Color.Black);
    Raylib.DrawLineEx(new Vector2(x - 6, y - 10 + bob), new Vector2(x + 6, y - 10 + bob), 2, new Color(100, 35, 48, 255));
    Raylib.DrawCircleV(new Vector2(x - 13, y - 13 + bob), 3, new Color(255, 145, 145, 255));
    Raylib.DrawCircleV(new Vector2(x + 13, y - 13 + bob), 3, new Color(255, 145, 145, 255));
}

void DrawGoal(float goalX)
{
    Raylib.DrawLineEx(new Vector2(goalX, groundY), new Vector2(goalX, groundY - 145), 7, new Color(92, 75, 59, 255));
    Raylib.DrawCircle((int)goalX, (int)(groundY - 148), 7, new Color(255, 214, 79, 255));
    Raylib.DrawTriangle(
        new Vector2(goalX + 4, groundY - 140),
        new Vector2(goalX + 78, groundY - 119),
        new Vector2(goalX + 4, groundY - 96),
        new Color(255, 190, 55, 255));
    Raylib.DrawText("GO!", (int)goalX + 12, (int)groundY - 130, 18, new Color(105, 67, 25, 255));
}

void DrawStickMan(Vector2 feet, float walkTime, bool jumping, bool facingRight, float squash)
{
    Color outline = new(24, 34, 48, 255);
    Color body = new(36, 48, 68, 255);
    float direction = facingRight ? 1f : -1f;
    float bob = jumping ? -2f : (float)Math.Sin(walkTime * 2f) * 2f;
    float headY = feet.Y - 61f + bob;
    float shoulderY = feet.Y - 43f + bob;
    float hipY = feet.Y - 22f + bob;

    Raylib.DrawEllipse((int)feet.X, (int)(feet.Y + 2), 17, 5, new Color(35, 67, 55, 75));

    // Legs animate in opposite directions.
    float legSwing = jumping ? 9f : (float)Math.Sin(walkTime) * 11f;
    Vector2 leftHip = new(feet.X - 4, hipY);
    Vector2 rightHip = new(feet.X + 4, hipY);
    Vector2 leftFoot = jumping
        ? new Vector2(feet.X - 13, feet.Y - 5)
        : new Vector2(feet.X - 7 + legSwing, feet.Y);
    Vector2 rightFoot = jumping
        ? new Vector2(feet.X + 14, feet.Y - 12)
        : new Vector2(feet.X + 7 - legSwing, feet.Y);

    Raylib.DrawLineEx(leftHip, leftFoot, 7, outline);
    Raylib.DrawLineEx(rightHip, rightFoot, 7, outline);
    Raylib.DrawLineEx(leftHip, leftFoot, 3, body);
    Raylib.DrawLineEx(rightHip, rightFoot, 3, body);

    // Torso and arms.
    Raylib.DrawLineEx(new Vector2(feet.X, shoulderY), new Vector2(feet.X, hipY), 9, outline);
    Raylib.DrawLineEx(new Vector2(feet.X, shoulderY), new Vector2(feet.X, hipY), 5, body);

    float armSwing = jumping ? -10f : (float)Math.Sin(walkTime + Math.PI) * 12f;
    Vector2 leftHand = new(feet.X - 15 + armSwing, shoulderY + 19);
    Vector2 rightHand = new(feet.X + 15 - armSwing, shoulderY + 19);
    if (jumping)
    {
        leftHand = new Vector2(feet.X - 19, shoulderY - 15);
        rightHand = new Vector2(feet.X + 19, shoulderY - 15);
    }

    Raylib.DrawLineEx(new Vector2(feet.X - 2, shoulderY + 2), leftHand, 6, outline);
    Raylib.DrawLineEx(new Vector2(feet.X + 2, shoulderY + 2), rightHand, 6, outline);
    Raylib.DrawLineEx(new Vector2(feet.X - 2, shoulderY + 2), leftHand, 3, body);
    Raylib.DrawLineEx(new Vector2(feet.X + 2, shoulderY + 2), rightHand, 3, body);

    // Head, face and hair tuft.
    Raylib.DrawCircleV(new Vector2(feet.X, headY), 13, outline);
    Raylib.DrawCircleV(new Vector2(feet.X, headY), 10, new Color(255, 205, 157, 255));
    Raylib.DrawCircleV(new Vector2(feet.X + direction * 4, headY - 1), 2, Color.Black);
    Raylib.DrawCircleV(new Vector2(feet.X + direction * 8, headY + 4), 1.5f, new Color(155, 75, 65, 255));
    Raylib.DrawLineEx(new Vector2(feet.X - 5, headY - 10), new Vector2(feet.X + 2, headY - 15), 3, outline);
}

void DrawOverlay(string title, string subtitle)
{
    Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(15, 27, 45, 175));
    int titleWidth = Raylib.MeasureText(title, 42);
    int subtitleWidth = Raylib.MeasureText(subtitle, 21);
    Raylib.DrawText(title, screenWidth / 2 - titleWidth / 2, 250, 42, new Color(255, 218, 100, 255));
    Raylib.DrawText(subtitle, screenWidth / 2 - subtitleWidth / 2, 315, 21, Color.White);
}

class Enemy
{
    public Vector2 Position;
    public bool IsSquashed;
    public float Direction = 1f;
    public float Speed = 115f;
    public float BobTime;

    private readonly float startX;
    private readonly float endX;
    private readonly float startY;

    public Enemy(float startX, float endX, float startY)
    {
        this.startX = startX;
        this.endX = endX;
        this.startY = startY;
        Reset();
    }

    public void Reset()
    {
        Position = new Vector2(startX, startY);
        IsSquashed = false;
        Direction = 1f;
        BobTime = 0f;
    }

    public void Update(float dt)
    {
        if (IsSquashed) return;

        Position.X += Speed * Direction * dt;
        if (Position.X >= endX)
        {
            Position.X = endX;
            Direction = -1f;
        }
        else if (Position.X <= startX)
        {
            Position.X = startX;
            Direction = 1f;
        }

        BobTime += dt;
    }
}

class Level
{
    public float WorldWidth;
    public float GoalX;
    public Vector2 StartPos;
    public Rectangle[] Platforms = Array.Empty<Rectangle>();
    public Rectangle[] Spikes = Array.Empty<Rectangle>();
    public List<Enemy> Enemies = new();
}
