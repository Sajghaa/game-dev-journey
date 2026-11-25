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

Raylib.InitWindow(screenWidth, screenHeight, "Stick Man Adventure - BOSS FIGHT!");
Raylib.SetTargetFPS(60);
Raylib.SetExitKey(KeyboardKey.Null); 

int deaths = 0;
int currentLevelIndex = 0;
int score = 0;
bool hasWon = false;
bool isPaused = false;
GameState gameState = GameState.Menu;

List<Level> levels = new()
{
    // LEVEL 1 (Same as before)
    new Level
    {
        WorldWidth = 3200f,
        GoalX = 3000f,
        StartPos = new Vector2(100, groundY),
        Platforms = new Rectangle[]
        {
            new(300, 440, 150, 22), new(560, 350, 150, 22), new(850, 440, 150, 22),
            new(1350, 420, 170, 22), new(1640, 330, 170, 22), new(1950, 420, 170, 22), new(2350, 360, 180, 22)
        },
        MovingPlatforms = new List<MovingPlatform>
        {
            new MovingPlatform(new Vector2(1150, 400), 150, 0, 100f, 2f),
            new MovingPlatform(new Vector2(2100, 350), 150, 200, 0f, 1.5f)
        },
        Spikes = new Rectangle[]
        {
            new(1050, groundY - 28, 130, 28), new(1780, groundY - 28, 120, 28), new(2150, groundY - 28, 140, 28)
        },
        Enemies = new List<Enemy>
        {
            new(650, 950, groundY - 22, EnemyType.Patroller),
            new(1450, 1700, groundY - 22, EnemyType.Spiky),
            new(2450, 2750, groundY - 22, EnemyType.Patroller),
            new(1800, 2000, 300, EnemyType.Flyer)
        },
        Coins = new List<Coin>
        {
            new(new Vector2(350, 400)), new(new Vector2(600, 310)), new(new Vector2(900, 400)), 
            new(new Vector2(1100, 480)), new(new Vector2(1200, 300)), new(new Vector2(1400, 380)), 
            new(new Vector2(1680, 290)), new(new Vector2(2000, 380)), new(new Vector2(2400, 320))
        },
        Checkpoints = new List<Checkpoint> { new Checkpoint(new Vector2(1600, groundY)) },
        Boss = null // No boss in level 1
    },
    // LEVEL 2 (Boss Level!)
    new Level
    {
        WorldWidth = 3600f,
        GoalX = 3400f,
        StartPos = new Vector2(100, groundY),
        Platforms = new Rectangle[]
        {
            new(250, 450, 130, 22), new(470, 360, 130, 22), new(700, 270, 130, 22),
            new(1100, 430, 180, 22), new(1430, 340, 150, 22), new(1700, 250, 150, 22),
            new(2500, 330, 160, 22), new(2800, 420, 160, 22)
        },
        MovingPlatforms = new List<MovingPlatform>
        {
            new MovingPlatform(new Vector2(1950, 400), 150, 0, 150f, 2.5f),
            new MovingPlatform(new Vector2(2300, 300), 120, 100, 0f, 2f)
        },
        Spikes = new Rectangle[]
        {
            new(870, groundY - 28, 180, 28), new(1870, groundY - 28, 180, 28), 
            new(2350, groundY - 28, 130, 28), new(3000, groundY - 28, 100, 28),
            new(2550, 280, 100, 20) 
        },
        Enemies = new List<Enemy>
        {
            new(1150, 1350, groundY - 22, EnemyType.Flyer),
            new(2200, 2450, groundY - 22, EnemyType.Spiky),
            new(2850, 3100, groundY - 22, EnemyType.Patroller)
        },
        Coins = new List<Coin>
        {
            new(new Vector2(280, 410)), new(new Vector2(500, 320)), new(new Vector2(730, 230)), 
            new(new Vector2(900, 480)), new(new Vector2(1150, 390)), new(new Vector2(1480, 300))
        },
        Checkpoints = new List<Checkpoint> { new Checkpoint(new Vector2(1000, groundY)), new Checkpoint(new Vector2(2600, groundY)) },
        // NEW: Spawning the Boss at the end of the level
        Boss = new Boss(new Vector2(3050, groundY - 100)) 
    }
};

Level currentLevel = levels[currentLevelIndex];
Vector2 position = currentLevel.StartPos;
Vector2 respawnPosition = currentLevel.StartPos;
float velocityY = 0f;
float walkTime = 0f;
float jumpSquash = 0f;
bool isFacingRight = true;

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
    Vector2 mouse = Raylib.GetMousePosition();

    // ---------------- MENU NAVIGATION ----------------
    if (gameState == GameState.Menu)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || IsButtonClicked(new Rectangle(390, 260, 220, 58), mouse))
        {
            currentLevelIndex = 0;
            ResetLevel(levels[currentLevelIndex]);
            gameState = GameState.Playing;
        }
        else if (IsButtonClicked(new Rectangle(390, 375, 220, 58), mouse)) gameState = GameState.LevelSelect;
        else if (IsButtonClicked(new Rectangle(390, 450, 220, 58), mouse)) gameState = GameState.HowToPlay;
    }
    else if (gameState == GameState.LevelSelect)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || IsButtonClicked(new Rectangle(30, 30, 145, 48), mouse))
            gameState = GameState.Menu;

        if (IsButtonClicked(new Rectangle(300, 285, 180, 100), mouse) || Raylib.IsKeyPressed(KeyboardKey.One))
        {
            currentLevelIndex = 0;
            ResetLevel(levels[currentLevelIndex]);
            gameState = GameState.Playing;
        }
        else if (IsButtonClicked(new Rectangle(520, 285, 180, 100), mouse) || Raylib.IsKeyPressed(KeyboardKey.Two))
        {
            currentLevelIndex = 1;
            ResetLevel(levels[currentLevelIndex]);
            gameState = GameState.Playing;
        }
    }
    else if (gameState == GameState.HowToPlay)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsKeyPressed(KeyboardKey.Enter) ||
            IsButtonClicked(new Rectangle(390, 500, 220, 55), mouse)) gameState = GameState.Menu;
    }
    else if (gameState == GameState.Playing)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape)) { gameState = GameState.Menu; isPaused = false; }
        else if (Raylib.IsKeyPressed(KeyboardKey.P)) isPaused = !isPaused;
    }

    if (gameState == GameState.Playing && hasWon)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.M)) { gameState = GameState.Menu; hasWon = false; }
        else if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            currentLevelIndex = (currentLevelIndex + 1) % levels.Count;
            ResetLevel(levels[currentLevelIndex]);
        }
    }
    else if (gameState == GameState.Playing && !isPaused && !hasWon)
    {
        // 1. UPDATE PLATFORMS
        foreach (MovingPlatform mp in currentLevel.MovingPlatforms) mp.Update(dt);

        // 2. PLAYER MOVEMENT
        float dx = 0f;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) { dx -= 1f; isFacingRight = false; }
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) { dx += 1f; isFacingRight = true; }

        position.X += dx * moveSpeed * dt;
        position.X = Math.Clamp(position.X, 20f, currentLevel.WorldWidth - 20f);

        bool onGround = position.Y >= groundY - 1f;
        if (onGround) { position.Y = groundY; velocityY = Math.Max(0f, velocityY); }

        Rectangle playerFeet = new(position.X - 13f, position.Y - 7f, 26f, 14f);
        foreach (Rectangle platform in currentLevel.Platforms)
            if (Raylib.CheckCollisionRecs(playerFeet, new Rectangle(platform.X, platform.Y - 4f, platform.Width, 8f))) onGround = true;

        foreach (MovingPlatform mp in currentLevel.MovingPlatforms)
        {
            if (Raylib.CheckCollisionRecs(playerFeet, new Rectangle(mp.Bounds.X, mp.Bounds.Y - 4f, mp.Bounds.Width, 8f)))
            {
                onGround = true;
                position.X += mp.Delta.X;
                position.Y += mp.Delta.Y;
            }
        }

        if (onGround && (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up)))
        {
            velocityY = jumpForce;
            onGround = false;
        }

        float previousY = position.Y;
        velocityY += gravity * dt;
        position.Y += velocityY * dt;

        if (velocityY >= 0f)
        {
            foreach (Rectangle platform in currentLevel.Platforms)
                if (position.X + 13f > platform.X && position.X - 13f < platform.X + platform.Width && previousY <= platform.Y && position.Y >= platform.Y)
                    { position.Y = platform.Y; velocityY = 0f; onGround = true; }
            
            foreach (MovingPlatform mp in currentLevel.MovingPlatforms)
                if (position.X + 13f > mp.Bounds.X && position.X - 13f < mp.Bounds.X + mp.Bounds.Width && previousY <= mp.Bounds.Y && position.Y >= mp.Bounds.Y)
                    { position.Y = mp.Bounds.Y; velocityY = 0f; onGround = true; }
        }
        if (position.Y >= groundY) { position.Y = groundY; velocityY = 0f; onGround = true; }

        bool inAir = !onGround;
        if (dx != 0f && !inAir) walkTime += dt * 11f;
        jumpSquash += ((inAir ? 0.12f : 0f) - jumpSquash) * Math.Min(1f, dt * 8f);

        // 3. COLLISION & HAZARDS
        Rectangle playerRect = new(position.X - 14f, position.Y - 72f, 28f, 68f);
        bool hitHazard = false;

        foreach (Rectangle spike in currentLevel.Spikes)
            if (Raylib.CheckCollisionRecs(playerRect, new Rectangle(spike.X + 5f, spike.Y + 8f, spike.Width - 10f, spike.Height - 8f))) hitHazard = true;

        foreach (Enemy enemy in currentLevel.Enemies)
        {
            if (enemy.IsSquashed) continue;
            Rectangle enemyRect = new(enemy.Position.X - 19f, enemy.Position.Y - 34f, 38f, 34f);
            if (Raylib.CheckCollisionRecs(playerRect, enemyRect))
            {
                bool stomping = velocityY > 0f && previousY - 72f <= enemyRect.Y + 12f;
                if (stomping && enemy.Type != EnemyType.Spiky) { enemy.IsSquashed = true; velocityY = -440f; score += 50; }
                else hitHazard = true;
            }
        }

        // NEW: BOSS COLLISION & PROJECTILES
        if (currentLevel.Boss != null && !currentLevel.Boss.IsDead)
        {
            currentLevel.Boss.Update(dt, position, groundY);
            
            // Boss collision with player
            Rectangle bossRect = new(currentLevel.Boss.Position.X - 50f, currentLevel.Boss.Position.Y - 80f, 100f, 80f);
            if (Raylib.CheckCollisionRecs(playerRect, bossRect))
            {
                bool stompingBoss = velocityY > 0f && previousY - 72f <= bossRect.Y + 15f;
                if (stompingBoss)
                {
                    currentLevel.Boss.TakeDamage();
                    velocityY = -550f; // Higher bounce off the boss
                }
                else
                {
                    hitHazard = true; // Touched the boss from the side
                }
            }

            // Projectiles hitting the player
            foreach (Projectile p in currentLevel.Boss.Projectiles)
            {
                if (Raylib.CheckCollisionCircleRec(p.Position, p.Radius, playerRect)) hitHazard = true;
            }
        }

        foreach (Coin coin in currentLevel.Coins)
            if (!coin.IsCollected && Vector2.Distance(position, coin.Position) < 45f) { coin.IsCollected = true; score += 10; }

        foreach (Checkpoint cp in currentLevel.Checkpoints)
            if (!cp.IsActivated && Math.Abs(position.X - cp.Position.X) < 30f && Math.Abs(position.Y - cp.Position.Y) < 70f)
            {
                cp.IsActivated = true;
                respawnPosition = new Vector2(cp.Position.X, groundY);
            }

        if (hitHazard)
        {
            position = respawnPosition;
            velocityY = 0f;
            deaths++;
            foreach (Enemy enemy in currentLevel.Enemies) enemy.Reset();
            // Don't reset the boss on death so you don't lose progress, but clear projectiles
            if (currentLevel.Boss != null) currentLevel.Boss.Projectiles.Clear(); 
        }

        foreach (Enemy enemy in currentLevel.Enemies) enemy.Update(dt);
        foreach (Coin coin in currentLevel.Coins) coin.Update(dt);

        // ONLY win if you reach the goal AND the boss is dead
        bool bossDefeated = currentLevel.Boss == null || currentLevel.Boss.IsDead;
        if (bossDefeated && position.X >= currentLevel.GoalX - 28f && position.Y >= groundY - 2f) 
            hasWon = true;
    }

    // Smooth camera follow.
    float cameraX = Math.Clamp(position.X, screenWidth / 2f, currentLevel.WorldWidth - screenWidth / 2f);
    camera.Target = Vector2.Lerp(camera.Target, new Vector2(cameraX, position.Y - 100f), 0.10f);

    // ---------------- DRAWING ----------------
    Raylib.BeginDrawing();

    if (gameState == GameState.Menu) DrawMainMenu();
    else if (gameState == GameState.LevelSelect) DrawLevelSelect();
    else if (gameState == GameState.HowToPlay) DrawHowToPlay();
    else
    {
        DrawSkyAndHud(currentLevelIndex + 1, deaths, currentLevel, position.X, score);

        Raylib.BeginMode2D(camera);
        DrawCartoonBackground(currentLevel.WorldWidth);
        DrawWorldGround(currentLevel.WorldWidth);

        foreach (Rectangle platform in currentLevel.Platforms) DrawGrassPlatform(platform);
        foreach (MovingPlatform mp in currentLevel.MovingPlatforms) DrawMovingPlatform(mp);
        foreach (Rectangle spike in currentLevel.Spikes) DrawCartoonSpikes(spike);
        foreach (Checkpoint cp in currentLevel.Checkpoints) DrawCheckpoint(cp);
        foreach (Enemy enemy in currentLevel.Enemies) DrawEnemy(enemy);
        foreach (Coin coin in currentLevel.Coins) DrawCoin(coin);

        // DRAW BOSS & PROJECTILES
        if (currentLevel.Boss != null && !currentLevel.Boss.IsDead)
        {
            currentLevel.Boss.Draw(groundY);
            foreach (Projectile p in currentLevel.Boss.Projectiles) DrawProjectile(p);
        }

        DrawGoal(currentLevel.GoalX, currentLevel.Boss == null || currentLevel.Boss.IsDead);
        DrawStickMan(position, walkTime, velocityY < 0f, isFacingRight, jumpSquash);
        Raylib.EndMode2D();

        // Draw Boss Health Bar (Screen Space)
        if (currentLevel.Boss != null && !currentLevel.Boss.IsDead && position.X > 2600)
            DrawBossHealthBar(currentLevel.Boss);

        if (isPaused && !hasWon) DrawOverlay("PAUSED", "Press P to resume  |  ESC for menu");
        if (hasWon) DrawOverlay($"LEVEL {currentLevelIndex + 1} COMPLETE!", "Press R for next level  |  M for menu");
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();

// ---------------- HELPERS & DRAWING ----------------

void ResetLevel(Level level)
{
    currentLevel = level;
    position = level.StartPos;
    respawnPosition = level.StartPos;
    velocityY = 0f; hasWon = false; isPaused = false; deaths = 0; score = 0;
    foreach (Enemy enemy in level.Enemies) enemy.Reset();
    foreach (Coin coin in level.Coins) coin.Reset();
    foreach (Checkpoint cp in level.Checkpoints) cp.IsActivated = false;
    if (level.Boss != null) level.Boss.Reset();
}

void DrawBossHealthBar(Boss boss)
{
    Raylib.DrawText("BOSS: SLIME KING", screenWidth / 2 - 100, 20, 20, Color.White);
    Raylib.DrawRectangle(screenWidth / 2 - 150, 45, 300, 25, new Color(50, 50, 50, 255));
    Raylib.DrawRectangle(screenWidth / 2 - 146, 49, (boss.Health * 73) - 8, 17, new Color(220, 50, 50, 255));
    Raylib.DrawRectangleLines(screenWidth / 2 - 150, 45, 300, 25, Color.White);
}

void DrawProjectile(Projectile p)
{
    Raylib.DrawCircleV(p.Position, p.Radius, new Color(255, 100, 0, 255));
    Raylib.DrawCircleV(p.Position, p.Radius - 3, new Color(255, 220, 50, 255));
}

void DrawSkyAndHud(int levelNumber, int deathCount, Level level, float playerX, int currentScore)
{
    Raylib.ClearBackground(new Color(116, 205, 255, 255));
    Raylib.DrawCircle(870, 95, 42, new Color(255, 226, 112, 255));
    DrawCloud(140, 105, 1f); DrawCloud(440, 75, 0.75f); DrawCloud(700, 165, 0.9f);
    Raylib.DrawRectangle(0, 0, screenWidth, 12, new Color(255, 255, 255, 70));

    Raylib.DrawRectangle(18, 18, 245, 110, new Color(25, 55, 82, 220));
    Raylib.DrawRectangle(18, 18, 245, 7, new Color(255, 210, 75, 255));
    Raylib.DrawText($"LEVEL {levelNumber}", 34, 34, 23, Color.White);
    Raylib.DrawText($"FALLS: {deathCount}", 35, 66, 18, new Color(255, 180, 180, 255));
    Raylib.DrawText($"SCORE: {currentScore}", 35, 92, 18, new Color(255, 215, 0, 255));

    float progress = Math.Clamp(playerX / level.GoalX, 0f, 1f);
    Raylib.DrawRectangle(700, 32, 260, 18, new Color(25, 55, 82, 210));
    Raylib.DrawRectangle(704, 36, (int)(252 * progress), 10, new Color(255, 210, 75, 255));
    Raylib.DrawText("ADVENTURE PROGRESS", 700, 12, 15, Color.White);
}

void DrawMovingPlatform(MovingPlatform mp)
{
    Raylib.DrawRectangleRec(mp.Bounds, new Color(120, 80, 160, 255));
    Raylib.DrawRectangle((int)mp.Bounds.X, (int)mp.Bounds.Y, (int)mp.Bounds.Width, 6, new Color(160, 120, 200, 255));
}

void DrawCheckpoint(Checkpoint cp)
{
    Color poleColor = new Color(100, 100, 100, 255);
    Color flagColor = cp.IsActivated ? new Color(255, 215, 0, 255) : new Color(150, 150, 150, 255);
    Raylib.DrawLineEx(new Vector2(cp.Position.X, cp.Position.Y), new Vector2(cp.Position.X, cp.Position.Y - 100), 5, poleColor);
    Raylib.DrawTriangle(new Vector2(cp.Position.X, cp.Position.Y - 95), new Vector2(cp.Position.X + 45, cp.Position.Y - 75), new Vector2(cp.Position.X, cp.Position.Y - 55), flagColor);
}

void DrawEnemy(Enemy enemy)
{
    float x = enemy.Position.X, y = enemy.Position.Y;
    if (enemy.IsSquashed) { Raylib.DrawEllipse((int)x, (int)(y - 4), 22, 7, new Color(151, 55, 68, 255)); return; }

    float bob = (float)Math.Sin(enemy.BobTime * 8f) * 2f;
    Raylib.DrawEllipse((int)x, (int)(y - 2), 23, 6, new Color(42, 75, 54, 90));

    if (enemy.Type == EnemyType.Patroller)
    {
        Raylib.DrawCircleV(new Vector2(x, y - 19 + bob), 19, new Color(218, 75, 91, 255));
        Raylib.DrawCircleV(new Vector2(x - 6, y - 23 + bob), 5, Color.White); Raylib.DrawCircleV(new Vector2(x + 6, y - 23 + bob), 5, Color.White);
    }
    else if (enemy.Type == EnemyType.Flyer)
    {
        Raylib.DrawCircleV(new Vector2(x, y - 19 + bob), 15, new Color(180, 80, 220, 255));
        Raylib.DrawCircleV(new Vector2(x - 5, y - 22 + bob), 4, Color.White); Raylib.DrawCircleV(new Vector2(x + 5, y - 22 + bob), 4, Color.White);
        Raylib.DrawTriangle(new Vector2(x - 10, y - 20 + bob), new Vector2(x - 25, y - 30 + bob), new Vector2(x - 15, y - 10 + bob), new Color(200, 150, 230, 255));
        Raylib.DrawTriangle(new Vector2(x + 10, y - 20 + bob), new Vector2(x + 25, y - 30 + bob), new Vector2(x + 15, y - 10 + bob), new Color(200, 150, 230, 255));
    }
    else if (enemy.Type == EnemyType.Spiky)
    {
        Raylib.DrawCircleV(new Vector2(x, y - 19 + bob), 19, new Color(60, 60, 60, 255));
        Raylib.DrawCircleV(new Vector2(x - 6, y - 23 + bob), 5, Color.Red); Raylib.DrawCircleV(new Vector2(x + 6, y - 23 + bob), 5, Color.Red);
        for (int i = -12; i <= 12; i += 8) Raylib.DrawTriangle(new Vector2(x + i, y - 30 + bob), new Vector2(x + i + 4, y - 45 + bob), new Vector2(x + i + 8, y - 30 + bob), Color.White);
    }

    float eyeDirection = enemy.Direction >= 0 ? 1f : -1f;
    Raylib.DrawCircleV(new Vector2(x - 6 + eyeDirection * 2, y - 23 + bob), 2.4f, Color.Black);
    Raylib.DrawCircleV(new Vector2(x + 6 + eyeDirection * 2, y - 23 + bob), 2.4f, Color.Black);
}

void DrawCoin(Coin coin)
{
    if (coin.IsCollected) return;
    float spinScale = Math.Abs((float)Math.Cos(coin.AnimTime));
    Vector2 pos = coin.Position; float bob = (float)Math.Sin(coin.AnimTime * 0.5f) * 4f;
    Raylib.DrawEllipse((int)pos.X, (int)(pos.Y + 15), 10, 4, new Color(35, 67, 55, 75));
    Raylib.DrawEllipse((int)pos.X, (int)(pos.Y + bob), 12 * spinScale, 14, new Color(255, 215, 0, 255));
    Raylib.DrawEllipseLines((int)pos.X, (int)(pos.Y + bob), 12 * spinScale, 14, new Color(184, 134, 11, 255));
}

void DrawGrassPlatform(Rectangle platform)
{
    Raylib.DrawRectangleRec(platform, new Color(151, 91, 51, 255));
    Raylib.DrawRectangle((int)platform.X, (int)platform.Y, (int)platform.Width, 9, new Color(66, 181, 76, 255));
    Raylib.DrawRectangle((int)platform.X, (int)platform.Y + 9, (int)platform.Width, 4, new Color(45, 145, 59, 255));
}

void DrawCartoonSpikes(Rectangle area)
{
    int count = Math.Max(1, (int)(area.Width / 24f)); float width = area.Width / count;
    for (int i = 0; i < count; i++)
    {
        Vector2 left = new(area.X + i * width, area.Y + area.Height);
        Vector2 top = new(area.X + i * width + width / 2f, area.Y + 2);
        Vector2 right = new(area.X + (i + 1) * width, area.Y + area.Height);
        Raylib.DrawTriangle(left, top, right, new Color(239, 76, 75, 255));
        Raylib.DrawLineEx(left, top, 2, new Color(255, 160, 145, 255));
        Raylib.DrawLineEx(top, right, 2, new Color(166, 43, 48, 255));
    }
}

// Updated to show locked state
void DrawGoal(float goalX, bool isUnlocked)
{
    Color poleColor = isUnlocked ? new Color(92, 75, 59, 255) : new Color(80, 80, 80, 255);
    Color flagColor = isUnlocked ? new Color(255, 190, 55, 255) : new Color(150, 150, 150, 255);
    
    Raylib.DrawLineEx(new Vector2(goalX, groundY), new Vector2(goalX, groundY - 145), 7, poleColor);
    Raylib.DrawCircle((int)goalX, (int)(groundY - 148), 7, flagColor);
    Raylib.DrawTriangle(new Vector2(goalX + 4, groundY - 140), new Vector2(goalX + 78, groundY - 119), new Vector2(goalX + 4, groundY - 96), flagColor);
    
    if (isUnlocked) Raylib.DrawText("GO!", (int)goalX + 12, (int)groundY - 130, 18, new Color(105, 67, 25, 255));
    else Raylib.DrawText("LOCKED", (int)goalX - 15, (int)groundY - 170, 16, Color.Red);
}

void DrawStickMan(Vector2 feet, float walkTime, bool jumping, bool facingRight, float squash)
{
    Color outline = new(24, 34, 48, 255); Color body = new(36, 48, 68, 255);
    float direction = facingRight ? 1f : -1f; float bob = jumping ? -2f : (float)Math.Sin(walkTime * 2f) * 2f;
    float headY = feet.Y - 61f + bob, shoulderY = feet.Y - 43f + bob, hipY = feet.Y - 22f + bob;

    Raylib.DrawEllipse((int)feet.X, (int)(feet.Y + 2), 17, 5, new Color(35, 67, 55, 75));
    float legSwing = jumping ? 9f : (float)Math.Sin(walkTime) * 11f;
    
    Vector2 leftHip = new(feet.X - 4, hipY), rightHip = new(feet.X + 4, hipY);
    Vector2 leftFoot = jumping ? new Vector2(feet.X - 13, feet.Y - 5) : new Vector2(feet.X - 7 + legSwing, feet.Y);
    Vector2 rightFoot = jumping ? new Vector2(feet.X + 14, feet.Y - 12) : new Vector2(feet.X + 7 - legSwing, feet.Y);

    Raylib.DrawLineEx(leftHip, leftFoot, 7, outline); Raylib.DrawLineEx(rightHip, rightFoot, 7, outline);
    Raylib.DrawLineEx(leftHip, leftFoot, 3, body); Raylib.DrawLineEx(rightHip, rightFoot, 3, body);
    Raylib.DrawLineEx(new Vector2(feet.X, shoulderY), new Vector2(feet.X, hipY), 9, outline);
    Raylib.DrawLineEx(new Vector2(feet.X, shoulderY), new Vector2(feet.X, hipY), 5, body);

    float armSwing = jumping ? -10f : (float)Math.Sin(walkTime + Math.PI) * 12f;
    Vector2 leftHand = jumping ? new Vector2(feet.X - 19, shoulderY - 15) : new Vector2(feet.X - 15 + armSwing, shoulderY + 19);
    Vector2 rightHand = jumping ? new Vector2(feet.X + 19, shoulderY - 15) : new Vector2(feet.X + 15 - armSwing, shoulderY + 19);

    Raylib.DrawLineEx(new Vector2(feet.X - 2, shoulderY + 2), leftHand, 6, outline); Raylib.DrawLineEx(new Vector2(feet.X + 2, shoulderY + 2), rightHand, 6, outline);
    Raylib.DrawLineEx(new Vector2(feet.X - 2, shoulderY + 2), leftHand, 3, body); Raylib.DrawLineEx(new Vector2(feet.X + 2, shoulderY + 2), rightHand, 3, body);
    
    Raylib.DrawCircleV(new Vector2(feet.X, headY), 13, outline);
    Raylib.DrawCircleV(new Vector2(feet.X, headY), 10, new Color(255, 205, 157, 255));
    Raylib.DrawCircleV(new Vector2(feet.X + direction * 4, headY - 1), 2, Color.Black);
}

// --- UI HELPERS ---
bool IsButtonClicked(Rectangle bounds, Vector2 mouse) => Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouse, bounds);
void DrawMenuButton(string label, Rectangle bounds, Vector2 mouse)
{
    bool hovered = Raylib.CheckCollisionPointRec(mouse, bounds);
    Color fill = hovered ? new Color(255, 196, 75, 255) : new Color(255, 218, 116, 255);
    Raylib.DrawRectangleRounded(bounds, 0.28f, 8, new Color(113, 70, 45, 255));
    Raylib.DrawRectangleRounded(new Rectangle(bounds.X + 3, bounds.Y + 3, bounds.Width - 6, bounds.Height - 7), 0.25f, 8, fill);
    Raylib.DrawText(label, (int)(bounds.X + (bounds.Width - Raylib.MeasureText(label, 23)) / 2), (int)(bounds.Y + 16), 23, new Color(64, 54, 50, 255));
}
void DrawMenuScenery()
{
    Raylib.ClearBackground(new Color(116, 205, 255, 255));
    Raylib.DrawCircle(850, 105, 52, new Color(255, 226, 112, 255));
    DrawCloud(135, 120, 1.15f); DrawCloud(760, 215, 0.85f); DrawCloud(420, 95, 0.7f);
    Raylib.DrawCircle(120, 610, 230, new Color(111, 190, 116, 255));
    Raylib.DrawRectangle(0, 570, screenWidth, 80, new Color(65, 177, 75, 255));
    Raylib.DrawRectangle(0, 585, screenWidth, 65, new Color(133, 83, 48, 255));
}
void DrawCloud(float x, float y, float scale)
{
    Color cloud = new(255, 255, 255, 235);
    Raylib.DrawCircleV(new Vector2(x, y), 23 * scale, cloud); Raylib.DrawCircleV(new Vector2(x + 25 * scale, y - 12 * scale), 30 * scale, cloud);
    Raylib.DrawCircleV(new Vector2(x + 55 * scale, y), 23 * scale, cloud); Raylib.DrawRectangle((int)(x - 2 * scale), (int)y, (int)(60 * scale), (int)(19 * scale), cloud);
}
void DrawMainMenu()
{
    Vector2 mouse = Raylib.GetMousePosition(); DrawMenuScenery();
    Raylib.DrawText("STICK MAN ADVENTURE", screenWidth / 2 - Raylib.MeasureText("STICK MAN ADVENTURE", 48) / 2, 110, 48, Color.White);
    DrawMenuButton("PLAY", new Rectangle(390, 260, 220, 58), mouse);
    DrawMenuButton("LEVEL SELECT", new Rectangle(390, 335, 220, 58), mouse);
    DrawMenuButton("HOW TO PLAY", new Rectangle(390, 410, 220, 58), mouse);
    DrawStickMan(new Vector2(255, 570), 2.4f, false, true, 0f);
    Raylib.DrawText("PRESS ENTER TO PLAY", 755, 620, 14, Color.White);
}
void DrawLevelSelect()
{
    Vector2 mouse = Raylib.GetMousePosition(); DrawMenuScenery();
    Raylib.DrawText("CHOOSE YOUR LEVEL", 300, 145, 38, Color.White);
    DrawMenuButton("< BACK", new Rectangle(30, 30, 145, 48), mouse);
    Rectangle first = new(300, 285, 180, 100), second = new(520, 285, 180, 100);
    Raylib.DrawRectangleRounded(first, 0.18f, 8, Raylib.CheckCollisionPointRec(mouse, first) ? new Color(255, 196, 75, 255) : new Color(255, 218, 116, 255));
    Raylib.DrawRectangleRounded(second, 0.18f, 8, Raylib.CheckCollisionPointRec(mouse, second) ? new Color(255, 196, 75, 255) : new Color(255, 218, 116, 255));
    Raylib.DrawText("LEVEL 1", 338, 310, 25, new Color(64, 54, 50, 255)); Raylib.DrawText("BEGINNER", 340, 345, 17, new Color(95, 75, 60, 255));
    Raylib.DrawText("LEVEL 2", 558, 310, 25, new Color(64, 54, 50, 255)); Raylib.DrawText("BOSS FIGHT", 558, 345, 17, Color.Red);
}
void DrawHowToPlay()
{
    DrawMenuScenery();
    Raylib.DrawText("HOW TO PLAY", 365, 120, 40, Color.White);
    Raylib.DrawRectangleRounded(new Rectangle(250, 205, 500, 245), 0.08f, 10, new Color(25, 55, 82, 220));
    Raylib.DrawText("MOVE       A / D  or  LEFT / RIGHT", 295, 245, 21, Color.White);
    Raylib.DrawText("JUMP       SPACE / W / UP", 295, 295, 21, Color.White);
    Raylib.DrawText("PAUSE      P", 295, 345, 21, Color.White);
    Raylib.DrawText("DEFEAT THE BOSS BY STOMPING HIM 3 TIMES!", 295, 395, 16, new Color(255, 218, 100, 255));
    DrawMenuButton("BACK", new Rectangle(390, 500, 220, 55), Raylib.GetMousePosition());
}
void DrawOverlay(string title, string subtitle)
{
    Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(15, 27, 45, 175));
    Raylib.DrawText(title, screenWidth / 2 - Raylib.MeasureText(title, 42) / 2, 250, 42, new Color(255, 218, 100, 255));
    Raylib.DrawText(subtitle, screenWidth / 2 - Raylib.MeasureText(subtitle, 21) / 2, 315, 21, Color.White);
}
void DrawCartoonBackground(float worldWidth)
{
    for (int i = 0; i < (int)(worldWidth / 420f) + 2; i++) {
        float x = i * 420f; Raylib.DrawCircleV(new Vector2(x + 120, groundY - 55), 145, new Color(111, 190, 116, 255));
        Raylib.DrawCircleV(new Vector2(x + 310, groundY - 45), 125, new Color(92, 174, 105, 255));
    }
}
void DrawWorldGround(float worldWidth)
{
    Raylib.DrawRectangle(0, (int)groundY, (int)worldWidth, 120, new Color(133, 83, 48, 255));
    Raylib.DrawRectangle(0, (int)groundY, (int)worldWidth, 15, new Color(65, 177, 75, 255));
}

// ---------------- CLASSES ----------------

enum GameState { Menu, LevelSelect, HowToPlay, Playing }
enum EnemyType { Patroller, Flyer, Spiky }

class Enemy
{
    public Vector2 Position;
    public bool IsSquashed;
    public float Direction = 1f;
    public float Speed = 115f;
    public float BobTime;
    public EnemyType Type;

    private readonly float startX, endX, startY;

    public Enemy(float startX, float endX, float startY, EnemyType type)
    {
        this.startX = startX; this.endX = endX; this.startY = startY; Type = type; Reset();
    }

    public void Reset() { Position = new Vector2(startX, startY); IsSquashed = false; Direction = 1f; BobTime = 0f; }

    public void Update(float dt)
    {
        if (IsSquashed) return;
        Position.X += Speed * Direction * dt;
        if (Type == EnemyType.Flyer) Position.Y = startY + (float)Math.Sin(BobTime * 3f) * 60f;

        if (Position.X >= endX) { Position.X = endX; Direction = -1f; }
        else if (Position.X <= startX) { Position.X = startX; Direction = 1f; }
        BobTime += dt;
    }
}

class Coin
{
    public Vector2 Position; public bool IsCollected; public float AnimTime;
    public Coin(Vector2 position) { Position = position; IsCollected = false; AnimTime = 0f; }
    public void Reset() => IsCollected = false;
    public void Update(float dt) { if (!IsCollected) AnimTime += dt * 6f; }
}

class Checkpoint
{
    public Vector2 Position; public bool IsActivated;
    public Checkpoint(Vector2 pos) { Position = pos; IsActivated = false; }
}

class MovingPlatform
{
    public Rectangle Bounds; public Vector2 Delta;
    private Vector2 origin; private float offsetX, offsetY, speed, timer;

    public MovingPlatform(Vector2 startPos, float width, float offsetX, float offsetY, float speed)
    {
        origin = startPos; this.offsetX = offsetX; this.offsetY = offsetY; this.speed = speed;
        Bounds = new Rectangle(startPos.X, startPos.Y, width, 22); Delta = Vector2.Zero;
    }

    public void Update(float dt)
    {
        timer += dt * speed;
        Vector2 oldPos = new(Bounds.X, Bounds.Y);
        Bounds.X = origin.X + (float)Math.Sin(timer) * offsetX;
        Bounds.Y = origin.Y + (float)Math.Sin(timer) * offsetY;
        Delta = new Vector2(Bounds.X - oldPos.X, Bounds.Y - oldPos.Y);
    }
}

// NEW: PROJECTILE CLASS
class Projectile
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius = 12f;
    public bool IsActive = true;

    public Projectile(Vector2 startPos, Vector2 direction, float speed)
    {
        Position = startPos;
        Velocity = direction * speed;
    }

    public void Update(float dt)
    {
        Position += Velocity * dt;
    }
}

// NEW: BOSS CLASS
class Boss
{
    public Vector2 Position;
    public int Health = 3;
    public bool IsDead = false;
    public List<Projectile> Projectiles = new();
    
    private float moveTimer = 0f;
    private float attackTimer = 0f;
    private float hitFlashTimer = 0f;
    private float baseX, baseY;
    private float direction = 1f;

    public Boss(Vector2 startPos)
    {
        baseX = startPos.X; baseY = startPos.Y;
        Reset();
    }

    public void Reset()
    {
        Position = new Vector2(baseX, baseY);
        Health = 3;
        IsDead = false;
        Projectiles.Clear();
        moveTimer = 0f; attackTimer = 0f; hitFlashTimer = 0f; direction = 1f;
    }

    public void TakeDamage()
    {
        Health--;
        hitFlashTimer = 0.2f; 
        if (Health <= 0) IsDead = true;
    }

    // UPDATED: Added groundY parameter
    public void Update(float dt, Vector2 playerPos, float groundY)
    {
        if (IsDead) return;

        moveTimer += dt;
        Position.X += 150f * direction * dt; 
        if (Position.X > baseX + 200) direction = -1f;
        if (Position.X < baseX - 200) direction = 1f;

        Position.Y = baseY + (float)Math.Sin(moveTimer * 3f) * 15f;

        attackTimer += dt;
        if (attackTimer >= 2.0f) 
        {
            attackTimer = 0f;
            Vector2 bossCenter = new(Position.X, Position.Y - 40);
            Vector2 dir = Vector2.Normalize(playerPos - bossCenter);
            Projectiles.Add(new Projectile(bossCenter, dir, 350f));
        }

        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            Projectiles[i].Update(dt);
            if (Projectiles[i].Position.X < Position.X - 1000 || Projectiles[i].Position.X > Position.X + 1000 || Projectiles[i].Position.Y > groundY)
                Projectiles.RemoveAt(i);
        }

        if (hitFlashTimer > 0) hitFlashTimer -= dt;
    }

    // UPDATED: Added groundY parameter
    public void Draw(float groundY)
    {
        if (IsDead) return;

        Color bodyColor = hitFlashTimer > 0 ? Color.Red : new Color(40, 20, 80, 255); 
        float x = Position.X, y = Position.Y;

        Raylib.DrawEllipse((int)x, (int)(groundY - 2), 60, 12, new Color(0, 0, 0, 100));
        
        Raylib.DrawCircleV(new Vector2(x, y - 40), 60, bodyColor);
        Raylib.DrawRectangle((int)x - 60, (int)y - 40, 120, 40, bodyColor);

        Raylib.DrawRectangle((int)x - 30, (int)y - 110, 60, 20, Color.Gold);
        Raylib.DrawTriangle(new Vector2(x - 30, y - 110), new Vector2(x - 20, y - 130), new Vector2(x - 10, y - 110), Color.Gold);
        Raylib.DrawTriangle(new Vector2(x - 10, y - 110), new Vector2(x, y - 140), new Vector2(x + 10, y - 110), Color.Gold);
        Raylib.DrawTriangle(new Vector2(x + 10, y - 110), new Vector2(x + 20, y - 130), new Vector2(x + 30, y - 110), Color.Gold);

        Raylib.DrawCircleV(new Vector2(x - 20, y - 55), 12, Color.Red);
        Raylib.DrawCircleV(new Vector2(x + 20, y - 55), 12, Color.Red);
        Raylib.DrawCircleV(new Vector2(x - 20, y - 55), 6, Color.White);
        Raylib.DrawCircleV(new Vector2(x + 20, y - 55), 6, Color.White);
        Raylib.DrawCircleV(new Vector2(x - 20, y - 55), 3, Color.Black);
        Raylib.DrawCircleV(new Vector2(x + 20, y - 55), 3, Color.Black);

        Raylib.DrawRectangle((int)x - 20, (int)y - 25, 40, 15, Color.Black);
    }
}

class Level
{
    public float WorldWidth;
    public float GoalX;
    public Vector2 StartPos;
    public Rectangle[] Platforms = Array.Empty<Rectangle>();
    public List<MovingPlatform> MovingPlatforms = new();
    public Rectangle[] Spikes = Array.Empty<Rectangle>();
    public List<Enemy> Enemies = new();
    public List<Coin> Coins = new();
    public List<Checkpoint> Checkpoints = new();
    public Boss? Boss; // NEW
}