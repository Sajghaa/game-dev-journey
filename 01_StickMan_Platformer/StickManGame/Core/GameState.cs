using System.Numerics;
using Raylib_cs;
using StickManGame.Entities;
using StickManGame.Systems;
using StickManGame.World;

namespace StickManGame.Core;

public class GameManager
{
    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 650;
    public const float GroundY = 540f;

    public GameState CurrentState = GameState.Menu;
    public Level CurrentLevel;
    public Player Player;
    public Camera2D Camera;
    
    public int Score = 0;
    public int Deaths = 0;
    public int CurrentLevelIndex = 0;
    public bool IsPaused = false;
    public bool HasWon = false;
    
    public ParticleSystem Particles = new();
    public AudioManager Audio = new();

    public float ShakeTime = 0f;
    public float ShakeIntensity = 0f;

    public GameManager()
    {
        Camera = new Camera2D
        {
            Offset = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
            Target = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
            Rotation = 0f,
            Zoom = 1f
        };
        
        // Initialize with Level 1
        CurrentLevelIndex = 0;
        CurrentLevel = LevelData.GetLevel(0);
        Player = new Player(CurrentLevel.StartPos);
    }

    public void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            float dt = Math.Min(Raylib.GetFrameTime(), 0.033f);
            
            // Update Logic
            if (CurrentState == GameState.Playing && !IsPaused && !HasWon)
            {
                Player.Update(dt, CurrentLevel, this);
                CurrentLevel.Update(dt);
                Particles.Update(dt);
                CheckCollisions();
            }

            UpdateCamera(dt);

            // Draw Logic
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(116, 205, 255, 255));

            if (CurrentState == GameState.Playing)
            {
                Raylib.BeginMode2D(Camera);
                CurrentLevel.Draw();
                Player.Draw();
                Particles.Draw();
                Raylib.EndMode2D();
                
                HUD.Draw(this);
            }
            
            Raylib.EndDrawing();
        }
    }

    private void CheckCollisions()
    {
        // CollisionSystem.HandlePlayerEnemies(Player, CurrentLevel, this);
        // CollisionSystem.HandlePlayerCoins(Player, CurrentLevel, this);
    }

    private void UpdateCamera(float dt)
    {
        if (ShakeTime > 0f)
        {
            ShakeTime -= dt;
            float offsetX = (float)(Random.Shared.NextDouble() * 2 - 1) * ShakeIntensity;
            float offsetY = (float)(Random.Shared.NextDouble() * 2 - 1) * ShakeIntensity;
            Camera.Offset = new Vector2(ScreenWidth / 2f + offsetX, ScreenHeight / 2f + offsetY);
        }
        else
        {
            Camera.Offset = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
        }

        float cameraX = Math.Clamp(Player.Position.X, ScreenWidth / 2f, CurrentLevel.WorldWidth - ScreenWidth / 2f);
        Camera.Target = Vector2.Lerp(Camera.Target, new Vector2(cameraX, Player.Position.Y - 100f), 0.10f);
    }
}