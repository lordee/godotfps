using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class UIManager : Node
{
    public static MainMenu MainMenu;
    public static OptionsMenu OptionsMenu;
    public static Console Console;
    public static Lobby Lobby;

    private static Stack<IUIItem> Stack = new Stack<IUIItem>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MainMenu = GetNode("MainMenu") as MainMenu;
        OptionsMenu = GetNode("OptionsMenu") as OptionsMenu;
        Console = GetNode("Console") as Console;
        Lobby = GetNode("Console") as Lobby;
    }

    public static void MouseModeToggle()
    {
        if (UIOpen())
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
        }
        else
        {
            Input.SetMouseMode(Input.MouseMode.Captured);
        }
    }

    public static bool UIOpen()
    {
        return (Stack.Count > 0);
    }

    public static void Open(IUIItem i)
    {
        Stack.Push(i);
        i.Open();
        UIManager.MouseModeToggle();
    }

    public static void Close()
    {
        IUIItem i = Stack.Pop();
        i.Close();
        UIManager.MouseModeToggle();
    }

    public static void UI_Cancel()
    {
        IUIItem i = Stack.Peek();
        i.UI_Cancel();
    }

    public static void UI_Accept()
    {
        IUIItem i = Stack.Peek();
        i.UI_Accept();
    }

    public static void UI_Up()
    {
        IUIItem i = Stack.Peek();
        i.UI_Up();
    }

    public static void UI_Down()
    {
        IUIItem i = Stack.Peek();
        i.UI_Down();
    }

    public static void UI_ConsoleToggle()
    {
        IUIItem i = Stack.Peek();
        if (i.GetType() == typeof(Console))
        {
            UIManager.Close();
        }
        else
        {
            Open(Console);
        }
    }
}
