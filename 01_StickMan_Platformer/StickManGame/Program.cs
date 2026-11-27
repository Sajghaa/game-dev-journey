using Raylib_cs;
using StickManGame.Core;

namespace StickManGame;

public static class Program
{
    public static void Main()
    {
        Raylib.InitWindow(1000, 650, "Stick Man Adventure - Modularized");
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(KeyboardKey.Null);
        Raylib.InitAudioDevice();

        GameManager game = new GameManager();
        game.Run();

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }
}