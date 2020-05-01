using System;

public interface IUIItem
{
    void Open();
    void Close();
    void UI_Accept();
    void UI_Up();
    void UI_Down();
    void UI_Cancel();
}