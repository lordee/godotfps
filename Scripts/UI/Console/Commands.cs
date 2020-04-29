using Godot;
using System.Collections.Generic;


public class Commands
{
    Game _game;
    public Dictionary<string, CommandInfo> List;

    public Commands(Game game)
    {
        _game = game;
        List =
            new Dictionary<string, CommandInfo> {
            {
                "help",
                new CommandInfo {
                    HelpMessages = new string[] {
                        "'help' Lists all commands",
                        "'help <command>' Displays the help message for an individual command",
                    },
                    Function = (Args) => this.Help(Args)
                }
            },

            {
                "quit",
                new CommandInfo {
                    HelpMessages = new string[] {
                        "'quit' Immediately closes the game",
                    },
                    Function = (Args) => this.Quit(Args)
                }
            },

            {
                "fps_max",
                new CommandInfo {
                    HelpMessages = new string[] {
                        "'fps_max' Prints the current max fps",
                        "'fps_max <fps>' Sets the max fps",
                    },
                    Function = (Args) => Commands.FpsMax(Args)
                }
            },

        };
    }

    public delegate void CommandFunction(string[] Args);

	public struct CommandInfo
	{
		public string[] HelpMessages;
		public CommandFunction Function;
	}


	public void RunCommand(string Line)
	{

		string[] Split = Line.Split(null);
		if(Split.Length >= 1)
		{
			string Name = Split[0];

			string[] Args = new string[Split.Length - 1];
			for(int Index = 1; Index < Split.Length; Index += 1)
				Args[Index - 1] = Split[Index];

			foreach(KeyValuePair<string, CommandInfo> Command in this.List)
			{
				if(Command.Key == Name)
				{
					Command.Value.Function(Args);
					return;
				}
			}

			Console.ThrowPrint($"No command '{Name}', try running 'help' to view  a list of all commands");
		}
	}
    
	private static bool ArgCountMismatch(string[] Args, int Expected)
	{
		bool Mismatch = Args.Length != Expected;

		if(Mismatch)
			Console.ThrowPrint(
				$"Expected {Expected} arguments but received {Args.Length} arguments"
			);

		return Mismatch;
	}

    public void Quit(string[] Args)
    {
        _game.Quit();
    }


	public void Help(string[] Args)
	{
		if(Args.Length == 0)
		{
			Console.Print("All commands:");
			foreach(KeyValuePair<string, Commands.CommandInfo> Command in this.List)
			{
				foreach(string Message in Command.Value.HelpMessages)
					Console.Print($"  {Message}");
			}
		}
		else
		{
			if(ArgCountMismatch(Args, 1))
				return;

			string CommandName = Args[0];

			if(this.List.TryGetValue(CommandName, out var Command))
			{
				foreach(string Message in Command.HelpMessages)
					Console.Print($"  {Message}");
			}
			else
			{
				Console.ThrowPrint(
					$"No command '{Args[0]}', try running 'help' to view  a list of all commands"
				);
				return;
			}
		}
	}

	public static void FpsMax(string[] Args)
	{
		if(Args.Length == 0)
		{
			Console.Print($"Current max fps: {Engine.TargetFps}");
			return;
		}

		if(ArgCountMismatch(Args, 1))
			return;

		string TargetString = Args[0];

		if(int.TryParse(TargetString, out int Target) && Target >= 10)
			Engine.TargetFps = Target;
		else
			Console.ThrowPrint($"Invalid max fps {TargetString}");
	}
}
