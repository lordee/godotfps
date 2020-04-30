using Godot;
using System;

public class OptionsMenu : Control
{

    Container _controlsContainer;
    public override void _Ready()
    {
        _controlsContainer = GetNode("TabContainer/Controls/ControlsContainer") as Container;

        _controlsContainer.AddChild(AddControl("test", nameof(TestMethod)));
    }

    private HBoxContainer AddControl(string lblText, string func)
    {
        HBoxContainer hbox = new HBoxContainer();
        hbox.MarginLeft = 2;
        
        Label lbl = new Label();
        lbl.Text = lblText;
        hbox.AddChild(lbl);

        CheckBox cb = new CheckBox();
        cb.Connect("toggled", this, func);
        hbox.AddChild(cb);

        return hbox;
    }

    private void TestMethod(bool test)
    {
        GD.Print("clicked " + test);
    }
}
