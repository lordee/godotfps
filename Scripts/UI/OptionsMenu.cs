using Godot;
using System;

public class OptionsMenu : Control, IUIItem
{
    Container _controlsContainer;
    public override void _Ready()
    {
        _controlsContainer = GetNode("TabContainer/Controls/ControlsContainer") as Container;
    }

    private void _on_Save_pressed()
    {
        GD.Print("save");
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
