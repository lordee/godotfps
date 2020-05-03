using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class OptionsMenu : Control, IUIItem
{
    Container _controlsContainer;

    // mouse controls
    LineEdit _mSensitivity;
    CheckBox _mInvert;

    Dictionary<string, string> stringHistory = new Dictionary<string, string>();
    public override void _Ready()
    {
        _controlsContainer = GetNode("TabContainer/Controls/ControlsContainer") as Container;
        _mSensitivity = _controlsContainer.GetNode("SensitivityContainer/SensitivityValue") as LineEdit;
        _mInvert = _controlsContainer.GetNode("InvertMouseContainer/InvertMouseCheckBox") as CheckBox;
    }

    private void _on_Save_pressed()
    {
        Commands.SaveConfig();
    }

    private void LoadValues()
    {
        _mSensitivity.Text = Settings.Sensitivity.ToString();
        stringHistory.Add("msensitivity", Settings.Sensitivity.ToString());
        _mInvert.Pressed = Settings.InvertedMouse == -1 ? true : false;
    }

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
