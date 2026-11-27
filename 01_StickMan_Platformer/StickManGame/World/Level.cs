using System.Numerics;
using Raylib_cs;
using StickManGame.Entities;

namespace StickManGame.World;

public class Level
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
    public Boss? Boss;

    public void Update(float dt)
    {
        foreach (var mp in MovingPlatforms) mp.Update(dt);
        foreach (var e in Enemies) e.Update(dt, 0, 0); // Pass player pos here
        foreach (var c in Coins) c.Update(dt);
    }

    public void Draw()
    {
        // Draw background, platforms, spikes, etc.
    }
}