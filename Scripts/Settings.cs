using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

public class Settings
{
    [UserSetting(typeof(Settings), nameof(Deadzone))]
    public static float Deadzone { get; set; } = 0.25f;
    [UserSetting(typeof(Settings), nameof(Sensitivity))]
    public static float Sensitivity { get; set; } = 0.2f;
    [UserSetting(typeof(Settings), nameof(InvertedMouse))]
    public static bool InvertedMouse { 
        get {
            return _invertedMouse;
        }
        set {
            _invertedMouse = value;
            InvertMouseValue = value ? -1 : 1;
        } 
    }

    private static bool _invertedMouse = true;
    public static float InvertMouseValue = -1;
    public static string ConfigLocation = "config.cfg";
    public static bool MouseCursorVisible = true;
    private Game _game;

    public Settings(Game game)
    {
        _game = game;
    }

    public static void InvertMouse(bool val)
    {
        InvertedMouse = val;
    }

    public void LoadConfig()
    {
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

    public static void SaveConfig()
    {
        using (StreamWriter sw = new StreamWriter(Settings.ConfigLocation))
		{
			foreach (string a in InputMap.GetActions())
			{
				if (!a.Contains("ui_"))
				{
					Godot.Collections.Array actionList = InputMap.GetActionList(a);
					foreach(InputEvent ie in actionList)
					{
						if (ie is InputEventKey iek)
						{
							string key = OS.GetScancodeString(iek.Scancode).ToLower();
							sw.WriteLine("bind " + key + " " + a);
						}
						else if (ie is InputEventMouseButton iemb)
						{
							int btn = iemb.ButtonIndex;
							string key = KeyTypes.List.Where(e => (e.Value.Type == ButtonInfo.TYPE.MOUSEBUTTON
																|| e.Value.Type == ButtonInfo.TYPE.MOUSEWHEEL)
																&& (int)e.Value.ButtonValue == btn	
																).FirstOrDefault().Key.ToLower();
							sw.WriteLine("bind " + key + " " + a);
						}
						else if (ie is InputEventJoypadButton iejb)
						{
							int btn = iejb.ButtonIndex;
							string key = KeyTypes.List.Where(e => e.Value.Type == ButtonInfo.TYPE.CONTROLLERBUTTON
																&& (int)e.Value.ControllerButtonValue == btn	
																).FirstOrDefault().Key.ToLower();
							sw.WriteLine("bind " + key + " " + a);
						}
					}
				}
			}

			// do other settings
			var props = Assembly.GetExecutingAssembly().GetTypes()
					.SelectMany(t => t.GetProperties())
					.Where(m => m.GetCustomAttributes(typeof(UserSetting), false).Length > 0);

			foreach (var p in props)
			{
				sw.WriteLine(p.Name.ToLower() + " " + p.GetValue(null, null).ToString().ToLower());
			}
		}
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