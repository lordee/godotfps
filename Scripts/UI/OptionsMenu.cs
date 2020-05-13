using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class OptionsMenu : Control, IUIItem
{
    Game _game;
    Container _controlsContainer;
    Container _settingsContainer;

    // player controls
    List<LineEdit> _playerControls = new List<LineEdit>();

    // mouse controls
    LineEdit _mSensitivity;
    CheckBox _mInvert;

    Dictionary<string, string> stringHistory = new Dictionary<string, string>();
    public override void _Ready()
    {
        _game = GetTree().Root.GetNode("Game") as Game;
        _settingsContainer = GetNode("TabContainer/Settings/SettingsContainer") as Container;
        _controlsContainer = GetNode("TabContainer/Controls/ControlsContainer") as Container;
        _mSensitivity = _settingsContainer.GetNode("SensitivityContainer/SensitivityValue") as LineEdit;
        _mInvert = _settingsContainer.GetNode("InvertMouseContainer/InvertMouseCheckBox") as CheckBox;
    }

    public void Init()
    {
        AddControls();
    }

    private void AddControls()
    {
        foreach(KeyValuePair<string, CommandInfo> kvp in _game.Commands.List)
        {
            if (kvp.Value.CommandType == CT.PlayerController)
            {
                AddControl(kvp);
            }
        }
    }

    private void AddControl(KeyValuePair<string, CommandInfo> kvp)
    {
        HBoxContainer hb = new HBoxContainer();
        Label lbl = new Label();
        lbl.Text = kvp.Key;
        LineEdit le = new LineEdit();
        le.Name = kvp.Key;

        hb.AddChild(lbl);
        hb.AddChild(le);
        _controlsContainer.AddChild(hb);
        _playerControls.Add(le);
    }

    private void _on_Load_Defaults_pressed()
    {
        Settings.LoadDefaultConfig();
        LoadValues();
    }
    private void _on_Save_pressed()
    {
        Settings.Sensitivity = float.Parse(_mSensitivity.Text);
        Settings.InvertedMouse = _mInvert.Pressed;

        foreach (LineEdit le in _playerControls)
        {
            Bindings.Bind(le.Name, le.Text);
        }

        Commands.SaveConfig();
    }

    private void LoadValues()
    {
        _mSensitivity.Text = Settings.Sensitivity.ToString();
        UpdateHistory("msensitivity", Settings.Sensitivity.ToString());
        _mInvert.Pressed = Settings.InvertedMouse;
        UpdateHistory("minvertmouse", Settings.InvertedMouse.ToString());

        foreach (LineEdit le in _playerControls)
        {
            Godot.Collections.Array actionList = InputMap.GetActionList(le.Name);
            foreach(InputEvent ie in actionList)
            {
                if (ie is InputEventKey iek)
                {
                    // FIXME - this only does 1 key per command
                    string key = OS.GetScancodeString(iek.Scancode).ToLower();
                    le.Text = key;
                    break;
                }
                else if (ie is InputEventMouseButton iem)
                {
                    // FIXME - append mouse to index number, interpret it for save too
                    le.Text = iem.ButtonIndex.ToString();
                }
            }
        }
    }

    private void UpdateHistory(string key, string val)
    {
        if (!stringHistory.ContainsKey(key.ToLower()))
        {
            stringHistory.Add(key.ToLower(), val);
        }
        else
        {
            stringHistory[key.ToLower()] = val;
        }
    }

    // FIXME - filter control assignments to take single key press etc

    private void _on_text_changed_number_only(string text, string controlName)
    {
        System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*(\.?)(\d+)?$");
        System.Text.RegularExpressions.Match m = rx.Match(text);

        // FIXME - make control lookup better, stop hardcoding stuff
        LineEdit le = null;
        switch (controlName.ToLower())
        {
            case "msensitivity":
                le = _mSensitivity;
                break;
        }

        if (le == null)
        {
            Console.ThrowPrint($"Control '{controlName}' does not exist in OptionsMenu._on_text_changed_number_only()");
            return;
        }

        if (!stringHistory.ContainsKey(controlName.ToLower()))
        {
            stringHistory.Add(controlName.ToLower(), "");
        }

        if (m.Success)
        {
            stringHistory[controlName.ToLower()] = text;
        }
        else
        {
            // change back to old text
            le.Text = stringHistory[controlName.ToLower()];
        }

        le.CaretPosition = le.Text.Length();
    }

    public void Open()
    {
        LoadValues();
        this.Show();
    }

    public void Close()
    {
        this.Hide();
    }

    public void UI_Up()
    {

    }

    public void UI_Down()
    {

    }

    public void UI_Cancel()
    {
        UIManager.Close();
    }

    public void UI_Accept()
    {
        
    }
}
