using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class Settings
{
    public static float Deadzone = 0.25f;
    public static float Sensitivity = 0.2f;
    public static bool MouseCursorVisible = true;
    public static float InvertedMouse = -1;

    public static void InvertMouse(bool val)
    {
        InvertedMouse = val ? -1 : 1;
    }
}