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
GameState gameState = GameState.Menu;

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

    // ---------------- MENU NAVIGATION ----------------
    Vector2 mouse = Raylib.GetMousePosition();

    if (gameState == GameState.Menu)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            currentLevelIndex = 0;
            currentLevel = levels[currentLevelIndex];
            position = currentLevel.StartPos;
            velocityY = 0f;
            hasWon = false;
            isPaused = false;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
            gameState = GameState.Playing;
        }

        if (IsButtonClicked(new Rectangle(390, 260, 220, 58), mouse))
        {
            currentLevelIndex = 0;
            currentLevel = levels[currentLevelIndex];
            position = currentLevel.StartPos;
            velocityY = 0f;
            hasWon = false;
            isPaused = false;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
            gameState = GameState.Playing;
        }
        else if (IsButtonClicked(new Rectangle(390, 375, 220, 58), mouse))
        {
            gameState = GameState.LevelSelect;
        }
        else if (IsButtonClicked(new Rectangle(390, 450, 220, 58), mouse))
        {
            gameState = GameState.HowToPlay;
        }
    }
    else if (gameState == GameState.LevelSelect)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || IsButtonClicked(new Rectangle(30, 30, 145, 48), mouse))
            gameState = GameState.Menu;

        if (IsButtonClicked(new Rectangle(300, 285, 180, 100), mouse) || Raylib.IsKeyPressed(KeyboardKey.One))
        {
            currentLevelIndex = 0;
            currentLevel = levels[currentLevelIndex];
            position = currentLevel.StartPos;
            velocityY = 0f;
            hasWon = false;
            isPaused = false;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
            gameState = GameState.Playing;
        }
        else if (IsButtonClicked(new Rectangle(520, 285, 180, 100), mouse) || Raylib.IsKeyPressed(KeyboardKey.Two))
        {
            currentLevelIndex = 1;
            currentLevel = levels[currentLevelIndex];
            position = currentLevel.StartPos;
            velocityY = 0f;
            hasWon = false;
            isPaused = false;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
            gameState = GameState.Playing;
        }
    }
    else if (gameState == GameState.HowToPlay)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.Enter) ||
            IsButtonClicked(new Rectangle(390, 500, 220, 55), mouse))
            gameState = GameState.Menu;
    }
    else if (gameState == GameState.Playing)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            gameState = GameState.Menu;
            isPaused = false;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.P))
        {
            isPaused = !isPaused;
        }
    }

    if (gameState == GameState.Playing && hasWon)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.M))
        {
            gameState = GameState.Menu;
            hasWon = false;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            currentLevelIndex = (currentLevelIndex + 1) % levels.Count;
            currentLevel = levels[currentLevelIndex];
            position = currentLevel.StartPos;
            velocityY = 0f;
            hasWon = false;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
        }
    }
    else if (gameState == GameState.Playing && !isPaused && !hasWon)
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

    if (gameState == GameState.Menu)
    {
        DrawMainMenu();
    }
    else if (gameState == GameState.LevelSelect)
    {
        DrawLevelSelect();
    }
    else if (gameState == GameState.HowToPlay)
    {
        DrawHowToPlay();
    }
    else
    {
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
            DrawOverlay("PAUSED", "Press P to resume  |  ESC for menu");
        if (hasWon)
            DrawOverlay($"LEVEL {currentLevelIndex + 1} COMPLETE!", "Press R for next level  |  M for menu");
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

// ---------------- DRAWING HELPERS ----------------

bool IsButtonClicked(Rectangle bounds, Vector2 mouse)
{
    return Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, bounds);
}

void DrawMenuButton(string label, Rectangle bounds, Vector2 mouse)
{
    bool hovered = Raylib.CheckCollisionPointRec(mouse, bounds);
    Color fill = hovered ? new Color(255, 196, 75, 255) : new Color(255, 218, 116, 255);
    Color edge = new(113, 70, 45, 255);
    Raylib.DrawRectangleRounded(new Rectangle(bounds.X + 3, bounds.Y + 5, bounds.Width, bounds.Height), 0.28f, 8, new Color(35, 55, 70, 90));
    Raylib.DrawRectangleRounded(bounds, 0.28f, 8, edge);
    Raylib.DrawRectangleRounded(new Rectangle(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 7), 0.25f, 8, fill);
    int textWidth = Raylib.MeasureText(label, 23);
    Raylib.DrawText(label, (int)(bounds.X + (bounds.Width - textWidth) / 2), (int)(bounds.Y + 16), 23, new Color(64, 54, 50, 255));
}

void DrawMenuScenery()
{
    Raylib.ClearBackground(new Color(116, 205, 255, 255));
    Raylib.DrawCircle(850, 105, 52, new Color(255, 226, 112, 255));
    DrawCloud(135, 120, 1.15f);
    DrawCloud(760, 215, 0.85f);
    DrawCloud(420, 95, 0.7f);
    Raylib.DrawCircle(120, 610, 230, new Color(111, 190, 116, 255));
    Raylib.DrawCircle(430, 650, 260, new Color(92, 174, 105, 255));
    Raylib.DrawCircle(900, 620, 250, new Color(111, 190, 116, 255));
    Raylib.DrawRectangle(0, 570, screenWidth, 80, new Color(65, 177, 75, 255));
    Raylib.DrawRectangle(0, 585, screenWidth, 65, new Color(133, 83, 48, 255));
}

void DrawMainMenu()
{
    Vector2 mouse = Raylib.GetMousePosition();
    DrawMenuScenery();

    string title = "STICK MAN ADVENTURE";
    int titleWidth = Raylib.MeasureText(title, 48);
    Raylib.DrawText(title, screenWidth / 2 - titleWidth / 2 + 4, 115, 48, new Color(30, 66, 84, 100));
    Raylib.DrawText(title, screenWidth / 2 - titleWidth / 2, 110, 48, Color.White);
    string tagline = "JUMP • DODGE • EXPLORE!";
    int taglineWidth = Raylib.MeasureText(tagline, 20);
    Raylib.DrawText(tagline, screenWidth / 2 - taglineWidth / 2, 170, 20, new Color(45, 83, 91, 255));

    DrawMenuButton("PLAY", new Rectangle(390, 260, 220, 58), mouse);
    DrawMenuButton("LEVEL SELECT", new Rectangle(390, 335, 220, 58), mouse);
    DrawMenuButton("HOW TO PLAY", new Rectangle(390, 410, 220, 58), mouse);

    DrawStickMan(new Vector2(255, 570), 2.4f, false, true, 0f);
    Raylib.DrawText("v1.0  •  MADE WITH RAYLIB", 18, 620, 14, Color.White);
    Raylib.DrawText("PRESS ENTER TO PLAY", 755, 620, 14, Color.White);
}

void DrawLevelSelect()
{
    Vector2 mouse = Raylib.GetMousePosition();
    DrawMenuScenery();
    Raylib.DrawText("CHOOSE YOUR LEVEL", 300, 145, 38, Color.White);
    DrawMenuButton("< BACK", new Rectangle(30, 30, 145, 48), mouse);

    Rectangle first = new(300, 285, 180, 100);
    Rectangle second = new(520, 285, 180, 100);
    bool hoverFirst = Raylib.CheckCollisionPointRec(mouse, first);
    bool hoverSecond = Raylib.CheckCollisionPointRec(mouse, second);
    Raylib.DrawRectangleRounded(first, 0.18f, 8, hoverFirst ? new Color(255, 196, 75, 255) : new Color(255, 218, 116, 255));
    Raylib.DrawRectangleRounded(second, 0.18f, 8, hoverSecond ? new Color(255, 196, 75, 255) : new Color(255, 218, 116, 255));
    Raylib.DrawText("LEVEL 1", 338, 310, 25, new Color(64, 54, 50, 255));
    Raylib.DrawText("BEGINNER", 340, 345, 17, new Color(95, 75, 60, 255));
    Raylib.DrawText("LEVEL 2", 558, 310, 25, new Color(64, 54, 50, 255));
    Raylib.DrawText("CHALLENGE", 555, 345, 17, new Color(95, 75, 60, 255));
    Raylib.DrawText("Click a level or press 1 / 2", 365, 435, 20, Color.White);
}

void DrawHowToPlay()
{
    DrawMenuScenery();
    Raylib.DrawText("HOW TO PLAY", 365, 120, 40, Color.White);
    Raylib.DrawRectangleRounded(new Rectangle(250, 205, 500, 245), 0.08f, 10, new Color(25, 55, 82, 220));
    Raylib.DrawText("MOVE       A / D  or  LEFT / RIGHT", 295, 245, 21, Color.White);
    Raylib.DrawText("JUMP       SPACE / W / UP", 295, 295, 21, Color.White);
    Raylib.DrawText("PAUSE      P", 295, 345, 21, Color.White);
    Raylib.DrawText("STOMP ENEMIES FROM ABOVE!", 295, 395, 19, new Color(255, 218, 100, 255));
    DrawMenuButton("BACK", new Rectangle(390, 500, 220, 55), Raylib.GetMousePosition());
}

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

enum GameState
{
    Menu,
    LevelSelect,
    HowToPlay,
    Playing
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
