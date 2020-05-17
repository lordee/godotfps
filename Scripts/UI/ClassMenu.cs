using Godot;
using System;

public class ClassMenu : VBoxContainer, IUIItem
{

    public override void _Ready()
    {
    }

    private void _on_Button_Pressed(int classNum)
    {
        UIManager.Game.Commands.ChooseClass(classNum);
        UIManager.Close();
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
