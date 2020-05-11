using Godot;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System;

// FIXME - overload with action type
public delegate void CommandFunction(List<string> Args);
public delegate void CommandFunction2(float val);

public struct CommandInfo
{
	public string[] HelpMessages;
	public CommandFunction Function;
	public CommandFunction2 Function2;
	public string FunctionName;
	public CT CommandType;
}

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
                    Function = (Args) => this.Help(Args),
					FunctionName = nameof(this.Help),
					CommandType = CT.Command,
                }
            },

            {
                "quit",
                new CommandInfo {
                    HelpMessages = new string[] {
                        "'quit' Immediately closes the game",
                    },
                    Function = (Args) => this.Quit(Args),
					FunctionName = nameof(this.Quit),
					CommandType = CT.Command,
                }
            },
			{
                "bind",
                new CommandInfo {
                    HelpMessages = new string[] {
                        "'bind' Attach commands to keys 'bind w move_forward'",
                    },
                    Function = (Args) => this.Bind(Args),
					FunctionName = nameof(this.Bind),
					CommandType = CT.Command,
                }
            },

            {
                "fps_max",
                new CommandInfo {
                    HelpMessages = new string[] {
                        "'fps_max' Prints the current max fps",
                        "'fps_max <fps>' Sets the max fps",
                    },
                    Function = (Args) => Commands.FpsMax(Args),
					FunctionName = nameof(Commands.FpsMax),
					CommandType = CT.Command,
                }
            },
			{
				"move_forward",
				new CommandInfo {
					HelpMessages = new string[] {
						"'move_forward' Moves player forward"
					},
					Function2 = (Args) => PlayerController.MoveForward(Args),
					FunctionName = nameof(PlayerController.MoveForward),
					CommandType = CT.PlayerController,
				}
			},
			{
				"move_backward",
				new CommandInfo {
					HelpMessages = new string[] {
						"'move_backward' Moves player backward"
					},
					Function2 = (Args) => PlayerController.MoveBack(Args),
					FunctionName = nameof(PlayerController.MoveBack),
					CommandType = CT.PlayerController,
				}
			},
			{
				"move_left",
				new CommandInfo {
					HelpMessages = new string[] {
						"'move_left' Moves player left"
					},
					Function2 = (Args) => PlayerController.MoveLeft(Args),
					FunctionName = nameof(PlayerController.MoveLeft),
					CommandType = CT.PlayerController,
				}
			},
			{
				"move_right",
				new CommandInfo {
					HelpMessages = new string[] {
						"'move_right' Moves player right"
					},
					Function2 = (Args) => PlayerController.MoveRight(Args),
					FunctionName = nameof(PlayerController.MoveRight),
					CommandType = CT.PlayerController,
				}
			},
			{
				"jump",
				new CommandInfo {
					HelpMessages = new string[] {
						"'jump' Moves player upward"
					},
					Function2 = (Args) => PlayerController.Jump(Args),
					FunctionName = nameof(PlayerController.Jump),
					CommandType = CT.PlayerController,
				}
			},
			{
				"attack",
				new CommandInfo {
					HelpMessages = new string[] {
						"'attack' Makes player attack"
					},
					Function2 = (Args) => PlayerController.Attack(Args),
					FunctionName = nameof(PlayerController.Attack),
					CommandType = CT.PlayerController,
				}
			},
			{
				"look_up",
				new CommandInfo {
					HelpMessages = new string[] {
						"'look_up' Makes player look up"
					},
					Function2 = (Args) => PlayerController.LookUp(Args),
					FunctionName = nameof(PlayerController.LookUp),
					CommandType = CT.PlayerController,
				}
			},
			{
				"look_down",
				new CommandInfo {
					HelpMessages = new string[] {
						"'look_down' Makes player look down"
					},
					Function2 = (Args) => PlayerController.LookDown(Args),
					FunctionName = nameof(PlayerController.LookDown),
					CommandType = CT.PlayerController,
				}
			},
			{
				"look_right",
				new CommandInfo {
					HelpMessages = new string[] {
						"'look_right' Makes player look right"
					},
					Function2 = (Args) => PlayerController.LookRight(Args),
					FunctionName = nameof(PlayerController.LookRight),
					CommandType = CT.PlayerController,
				}
			},
			{
				"look_left",
				new CommandInfo {
					HelpMessages = new string[] {
						"'look_left' Makes player look left"
					},
					Function2 = (Args) => PlayerController.LookLeft(Args),
					FunctionName = nameof(PlayerController.LookLeft),
					CommandType = CT.PlayerController,
				}
			},
			{
				"mousemode_toggle",
				new CommandInfo {
					HelpMessages = new string[] {
						"'mousemode_toggle' Toggles mouse mode"
					},
					Function2 = (Args) => PlayerController.MouseModeToggle(),
					FunctionName = nameof(PlayerController.MouseModeToggle),
					CommandType = CT.Command,
				}
			},
			{
				"mainmenu_toggle",
				new CommandInfo {
					HelpMessages = new string[] {
						"'mainmenu_toggle' Toggles main menu"
					},
					Function2 = (Args) => MainMenu.MainMenuToggle(),
					FunctionName = nameof(MainMenu.MainMenuToggle),
					CommandType = CT.Command,
				}
			},
			{
				"console_toggle",
				new CommandInfo {
					HelpMessages = new string[] {
						"'console_toggle' Toggles console"
					},
					Function2 = (Args) => Console.ConsoleToggle(),
					FunctionName = nameof(Console.ConsoleToggle),
					CommandType = CT.Command,
				}
			},
			{
				"save_cfg",
				new CommandInfo {
					HelpMessages = new string[] {
						"'save_cfg' saves all current binds and settings to config.cfg"
					},
					Function = (Args) => Commands.SaveConfig(),
					FunctionName = nameof(Commands.SaveConfig),
					CommandType = CT.Command,
				}
			}
        };
    }
	
	public static void SaveConfig()
	{
		Settings.SaveConfig();
	}

	public void RunCommand(string Line)
	{
		var parsedText = ParseText(Line, ' ', '"');
		List<string> Args = parsedText.ToList();
		if(Args.Count >= 1)
		{
			string Name = Args[0];
			Args.RemoveAt(0);

			foreach(KeyValuePair<string, CommandInfo> Command in this.List)
			{
				if(Command.Key == Name)
				{
					Command.Value.Function(Args);
					return;
				}
			}

			var props = Assembly.GetExecutingAssembly().GetTypes()
					.SelectMany(t => t.GetProperties())
					.Where(m => m.GetCustomAttributes(typeof(UserSetting), false).Length > 0);

			foreach (var p in props)
			{
				if (p.Name.ToLower() == Name.ToLower())
				{
					if (p.PropertyType == typeof(String))
					{
						p.SetValue(null, Args[0]);
						return;
					}
					else if (p.PropertyType == typeof(float) || p.PropertyType == typeof(int)
							|| p.PropertyType == typeof(Single))
					{
						float f = 0;
						if (float.TryParse(Args[0], out f))
						{
							p.SetValue(null, f);
						}
						else
						{
							Console.ThrowPrint($"Could not parse '{Args[0]}' as a number");
						}
						return;
					}
					else if (p.PropertyType == typeof(Boolean)) // am i dumb? yes
					{
						bool b;
						int i;
						if (bool.TryParse(Args[0], out b))
						{
							p.SetValue(null, b);
						}
						else if (int.TryParse(Args[0], out i))
						{
							b = i == 1 ? true : false;
							p.SetValue(null, b);
						}
						else
						{
							Console.ThrowPrint($"Could not parse '{Args[0]}' as a bool");
						}
						return;
					}
				}
			}

			Console.ThrowPrint($"No command '{Name}', try running 'help' to view  a list of all commands");
		}
	}

	public static IEnumerable<string> ParseText(string line, char delimiter, char textQualifier)
	{
		if (line == null)
			yield break;

		else
		{
			char prevChar = '\0';
			char nextChar = '\0';
			char currentChar = '\0';

			bool inString = false;

			StringBuilder token = new StringBuilder();

			for (int i = 0; i < line.Length; i++)
			{
				currentChar = line[i];

				if (i > 0)
					prevChar = line[i - 1];
				else
					prevChar = '\0';

				if (i + 1 < line.Length)
					nextChar = line[i + 1];
				else
					nextChar = '\0';

				if (currentChar == textQualifier && (prevChar == '\0' || prevChar == delimiter) && !inString)
				{
					inString = true;
					continue;
				}

				if (currentChar == textQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
				{
					inString = false;
					continue;
				}

				if (currentChar == delimiter && !inString)
				{
					yield return token.ToString();
					token = token.Remove(0, token.Length);
					continue;
				}

				token = token.Append(currentChar);

			}

			yield return token.ToString();
		} 
	}
	
    
	private static bool ArgCountMismatch(List<string> Args, int Expected)
	{
		bool Mismatch = Args.Count != Expected;

		if(Mismatch)
			Console.ThrowPrint(
				$"Expected {Expected} arguments but received {Args.Count} arguments"
			);

		return Mismatch;
	}

    public void Quit(List<string> Args)
    {
        _game.Quit();
    }


	public void Help(List<string> Args)
	{
		if(Args.Count == 0)
		{
			Console.Print("All commands:");
			foreach(KeyValuePair<string, CommandInfo> Command in this.List)
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

	public void Bind(List<string> Args)
	{
		if (Args.Count == 1)
		{
			// return command assigned to key
			Godot.Collections.Array actions = InputMap.GetActions();
			if (actions.Count == 0)
			{
				Console.ThrowPrint($"'{Args[0]}' is unbound");
				return;
			}

			foreach (string a in actions)
			{
				if (!a.Contains("ui_"))
				{
					Godot.Collections.Array actionList = InputMap.GetActionList(a);
					foreach(InputEvent ie in actionList)
					{
						if (ie is InputEventKey iek)
						{
							string key = OS.GetScancodeString(iek.Scancode);
							if (key.ToLower() == Args[0].ToLower())
							{
								Console.Print($"'{Args[0]}' is bound to '{a}'");
								return;
							}
						}
					}
				}
			}
		}
		else if (Args.Count == 2)
		{
			// bind command to key
			Bindings.Bind(Args[1], Args[0]);
		}
		else if (Args.Count == 0)
		{
			Help(new List<string>() {"bind"});
		}
		else
		{
			Console.ThrowPrint("Too many arguments provided to bind command");
		}
	}

	public static void FpsMax(List<string> Args)
	{
		if(Args.Count == 0)
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


