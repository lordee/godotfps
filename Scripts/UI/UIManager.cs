using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class UIManager : Node
{
    private class UIItem
    {
        public string Type;
        public bool Open;
    }

    private static bool _menuOpen = false;
    private static bool _consoleOpen = false;
    private static List<UIItem> Items = new List<UIItem>();

    public static MainMenu MainMenu;
    public static Console Console;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MainMenu = GetNode("MainMenu") as MainMenu;
        Console = GetNode("Console") as Console;
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
        if (UIManager.Items.Where(e => e.Open == true).FirstOrDefault() == null)
            return false;
        
        return true;
    }

    public static void Open(string type)
    {
        UIItem item = UIManager.Items.Where(e => e.Type == type).FirstOrDefault();
        if (item == null)
        {
            UIManager.Items.Add(new UIItem { Type = type, Open = true });
        }
        else
        {
            item.Open = true;
        }
        UIManager.MouseModeToggle();
    }

    public static void Close(string type)
    {
        UIItem item = UIManager.Items.Where(e => e.Type == type).FirstOrDefault();
        if (item == null)
        {
            UIManager.Items.Add(new UIItem { Type = type, Open = false });
        }
        else
        {
            item.Open = false;
        }
        UIManager.MouseModeToggle();
    }
}
