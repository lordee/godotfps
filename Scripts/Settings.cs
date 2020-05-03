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
    public static string ConfigLocation = "config.cfg";
    private Game _game;

    public Settings(Game game)
    {
        _game = game;
    }

    public static void InvertMouse(bool val)
    {
        InvertedMouse = val ? -1 : 1;
    }

    public void LoadConfig()
    {
        // FIXME - debugging only
        LoadDefaultConfig();

        // TODO - load/save of config
        if (System.IO.File.Exists(ConfigLocation))
        {
            foreach(string line in System.IO.File.ReadLines(ConfigLocation))
            {
                _game.Commands.RunCommand(line);
            }
        }
        else
        {
            // if not exist, load defaults
            LoadDefaultConfig();
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        /*using (System.IO.StreamWriter writer = new System.IO.StreamWriter(ConfigLocation))
        {
            foreach (string s in bindlist)
            {
                writer.WriteLine(s);
            }
        }*/
    }

    private void LoadDefaultConfig()
    {
        // kb move
        Bindings.Bind("move_forward", "W");
        Bindings.Bind("move_left", "A");
        Bindings.Bind("move_backward", "S");
        Bindings.Bind("move_right", "D");
        Bindings.Bind("jump", "Space");

        // mouse
        Bindings.Bind("attack", "MouseOne");
        Bindings.Bind("look_up", "MouseUp");
        Bindings.Bind("look_down", "MouseDown");
        Bindings.Bind("look_right", "MouseRight");
        Bindings.Bind("look_left", "MouseLeft");

        // other
        Bindings.Bind("mousemode_toggle", "M");
        Bindings.Bind("mainmenu_toggle", "Escape");
        Bindings.Bind("console_toggle", "`");
    }
}