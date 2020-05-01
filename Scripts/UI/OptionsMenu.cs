using Godot;
using System;

public class OptionsMenu : Control, IUIItem
{

    Container _controlsContainer;
    public override void _Ready()
    {
        _controlsContainer = GetNode("TabContainer/Controls/ControlsContainer") as Container;

        HBoxContainer invertMouse = AddControl("Invert Mouse", nameof(InvertMouseToggle), (Settings.InvertedMouse == -1) ? true : false);
        _controlsContainer.AddChild(invertMouse);

    }

    private HBoxContainer AddControl(string lblText, string func, bool val)
    {
        HBoxContainer hbox = new HBoxContainer();
        hbox.MarginLeft = 2;
        
        Label lbl = new Label();
        lbl.Text = lblText;
        hbox.AddChild(lbl);

        CheckBox cb = new CheckBox();
        cb.Connect("toggled", this, func);
        hbox.AddChild(cb);
        cb.Pressed = val;

        return hbox;
    }

    // FIXME - should not need this method
    private void InvertMouseToggle(bool val)
    {
        Settings.InvertMouse(val);
    }

    public void Open()
    {
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
