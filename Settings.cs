using System;
using Microsoft.Xna.Framework;

namespace FireInTheHole;

// Should read from an ini file
public static class Settings
{
    public static int FramesPerSecond = 70;
    public static int ScreenWidth = 1600;
    public static int ScreenHalfWidth = ScreenWidth / 2;
    public static int ScreenHeight = 900;
    public static int ScreenHalfHeight = ScreenHeight / 2;
    public static int ScreenQuarterHeight = ScreenHeight / 4;
    public static bool IsFullScreen = false;
    public static bool UseMapRenderer = false;
    public static float PlayerRotationSpeed = 0.15f;
    public static float PlayerMoveSpeed = 0.2f;
    public static int PlayerRayCount = ScreenWidth / 2;
    public static int PlayerRayMaxLength = 30;
    public static int PlayerHalfRayCount = PlayerRayCount / 2;
    public static int PlayerScale = 5;
    public static float PlayerFovInDegrees = 90;
    public static float PlayerHalfFovInDegrees = PlayerFovInDegrees / 2;
    public static float PlayerDeltaFovInDegrees = PlayerFovInDegrees / PlayerRayCount;
    public static float PlayerScreenDistance = ScreenHalfWidth / MathF.Tan(MathHelper.ToRadians(PlayerHalfFovInDegrees));
    public static float PlayerDropDynamiteTime = 0.5f;
    public static float DynamiteFuseTime = 3f;
    public static float DynamiteExplosionTime = 1f;
    public static bool DynamiteOrthoganalExplosions = true;
    public static int DynamiteSize = 1;
    public static int TileScale = ScreenWidth / PlayerRayCount;
    public static int PlayerCount = 4; // 2-4 until I implement a single player mode
    public static float PlayerAnimationTime = 0.3f;
    public static float PlayerSize = 10f;
}