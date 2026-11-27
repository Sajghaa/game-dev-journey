using System.Numerics;
using StickManGame.Core;

namespace StickManGame.Entities;

public enum EnemyType { Patroller, Flyer, Spiky, Swordsman }

public class Enemy
{
    public Vector2 Position;
    public EnemyType Type;
    public bool IsSquashed;
    public float Direction = 1f;
    public bool IsAttacking = false;
    
    
}