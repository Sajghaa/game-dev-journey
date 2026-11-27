using System.Numerics;
using Raylib_cs;
using StickManGame.Core;

namespace StickManGame.Entities;

public class Player
{
    public Vector2 Position;
    public float VelocityY = 0f;
    public int Health = 3;
    public int JumpsRemaining = 1;
    public float InvincibilityTimer = 0f;
    public bool IsFacingRight = true;
    public float SlashTimer = 0f;

    public Player(Vector2 startPos)
    {
        Position = startPos;
    }

    public void Update(float dt, World.Level level, GameManager game)
    {
        // ... (Move your player movement, jump, gravity, and sword attack logic here)
        // Use game.Audio.PlayJump(), etc.
    }

    public void Draw()
    {
        // ... (Move your DrawStickMan logic here)
    }
}