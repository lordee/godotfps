using Godot;
using System;
using System.Collections.Generic;

public class Console : Panel
{
	public static bool IsOpen = false;

    public static Game _game;

	public static LineEdit InputLine;
	public static RichTextLabel ConsoleLabel;
	public static RichTextLabel LogLabel;
	public static List<string> History = new List<string>();
	public static int HistoryLocation = 0;

	public override void _Ready()
	{
		InputLine = GetNode("VBox/LineEdit") as LineEdit;
		Close(); //Console.IsOpen should already be false

		_game = GetTree().Root.GetNode("Game") as Game;
		ConsoleLabel = GetNode("VBox/HBox/Console") as RichTextLabel;
		LogLabel = GetNode<RichTextLabel>("VBox/HBox/Log");
		Console.Print("");
		LogLabel.Text += "\n";
	}

	public override void _Process(float Delta)
	{
		if(Input.IsActionJustPressed("Enter") && IsOpen)
		{
			this.Execute(InputLine.Text);
			InputLine.Text = "";
		}
	}

	public override void _Input(InputEvent Event)
	{
		if(Event.IsAction("ui_up"))
		{
			GetTree().SetInputAsHandled();
			InputLine.GrabFocus();

			if(Input.IsActionJustPressed("ui_up") && HistoryLocation > 0)
			{
				HistoryLocation -= 1;
				InputLine.Text = History[HistoryLocation];

				InputLine.CaretPosition = InputLine.Text.Length;
			}
		}

		if(Event.IsAction("ui_down"))
		{
			GetTree().SetInputAsHandled();
			InputLine.GrabFocus();

			if(Input.IsActionJustPressed("ui_down") && HistoryLocation < History.Count)
			{
				HistoryLocation += 1;
				if(HistoryLocation == History.Count)
				{
					InputLine.Text = "";
				}
				else
				{
					InputLine.Text = History[HistoryLocation];
				}

				InputLine.CaretPosition = InputLine.Text.Length;
			}
		}
	}

	public void Open()
	{
		this.Show();
		IsOpen = true;
		InputLine.Editable = true;
		InputLine.Text = "";
		InputLine.GrabFocus();

		HistoryLocation = History.Count;

		UIManager.Open(nameof(Console));
	}

	public void Close()
	{
		this.Hide();
		IsOpen = false;
		InputLine.Editable = false;
		InputLine.Text = "";
		HistoryLocation = History.Count;

		UIManager.Close(nameof(Console));
	}

	public static void Print(object ToPrint)
	{
		ConsoleLabel.Text += $"{ToPrint}\n";
	}


	public static void Log(object ToLog)
	{
		LogLabel.Text += $"{ToLog}\n\n";
	}


	public static void ThrowPrint(object ToThrow)
	{
		Print($"ERROR: {ToThrow}");
	}


	public static void ThrowLog(object ToThrow)
	{
		Log($"ERROR: {ToThrow}");
	}


	public void Execute(string Command)
	{
		Console.Print("\n>>> " + Command);

		if(History.Count <= 0 || History[History.Count-1] != Command)
		{
			History.Add(Command);
		}
		HistoryLocation = History.Count;

		_game.Commands.RunCommand(Command);
	}

	[InputWithoutArg(typeof(Console), nameof(ConsoleToggle))]
	public static void ConsoleToggle()
	{
		IsOpen = !IsOpen;
		if (IsOpen)
		{
			UIManager.Console.Open();
			UIManager.Open(nameof(Console));
		}
		else
		{
			UIManager.Console.Close();
			UIManager.Close(nameof(Console));
		}
	}
}