using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;



public class Bindings : Node
{
	public struct WithArgInfo
	{
		public string Name;
		public InputWithArg Tag;

		public WithArgInfo(string NameArg, InputWithArg TagArg)
		{
			Name = NameArg;
			Tag = TagArg;
		}
	}

	public struct WithArgCommandInfo
	{
		public string Name;
		public CommandWithArg Tag;

		public WithArgCommandInfo(string NameArg, CommandWithArg TagArg)
		{
			Name = NameArg;
			Tag = TagArg;
		}
	}

	public struct WithoutArgInfo
	{
		public string Name;
		public InputWithoutArg Tag;

		public WithoutArgInfo(string NameArg, InputWithoutArg TagArg)
		{
			Name = NameArg;
			Tag = TagArg;
		}
	}

	private static List<WithArgCommandInfo> WithArgCommands = null;
	private static List<WithArgInfo> WithArgMethods = null;
	private static List<WithoutArgInfo> WithoutArgMethods = null;

	public static List<BindingObject> BindingsWithArg = new List<BindingObject>();
	private static List<BindingObject> BindingsWithoutArg = new List<BindingObject>();

	private static Bindings Self;
	private static Game _game;
	private Bindings()
	{
		Self = this;
	}

	public override void _Ready()
    {
		_game = GetTree().Root.GetNode("Game") as Game;
	}


	public static bool Bind(string actionName, string KeyName)
	{
		string FunctionName;
		actionName = actionName.ToLower();
		// TODO - support multiple commands with semicolon
		string searchname = actionName.Split(" ")[0];
		KeyName = KeyName.ToLower();
		
		var kvp = _game.Commands.List.Where(e => e.Key.ToLower() == searchname).FirstOrDefault();
		CommandInfo ci = kvp.Value;
		FunctionName = ci.FunctionName;
		
		BindingObject NewBind = new BindingObject(actionName, KeyName);

		bool Found = false;

		if(WithArgMethods == null)
		{
			var Methods = Assembly.GetExecutingAssembly().GetTypes()
				.SelectMany(t => t.GetMethods())
				.Where(m => m.GetCustomAttributes(typeof(InputWithArg), false).Length > 0);

			WithArgMethods = new List<WithArgInfo>();
			foreach(MethodInfo Method in Methods)
			{
				WithArgMethods.Add(
					new WithArgInfo(
						Method.Name,
						Attribute.GetCustomAttribute(Method, typeof(InputWithArg)) as InputWithArg
					)
				);
			}
		}
		foreach(WithArgInfo Method in WithArgMethods)
		{
			if(Method.Name == FunctionName)
			{
				Found = true;
				NewBind.FuncWithArg = Method.Tag.Function;
				break;
			}
		}

		if(WithoutArgMethods == null)
		{
			var Methods = Assembly.GetExecutingAssembly().GetTypes()
				.SelectMany(t => t.GetMethods())
				.Where(m => m.GetCustomAttributes(typeof(InputWithoutArg), false).Length > 0);

			WithoutArgMethods = new List<WithoutArgInfo>();
			foreach(MethodInfo Method in Methods)
			{
				WithoutArgMethods.Add(
					new WithoutArgInfo(
						Method.Name,
						Attribute.GetCustomAttribute(Method, typeof(InputWithoutArg)) as InputWithoutArg
					)
				);
			}
		}
		foreach(WithoutArgInfo Method in WithoutArgMethods)
		{
			if(Method.Name == FunctionName)
			{
				Found = true;
				NewBind.FuncWithoutArg = Method.Tag.Function;
				break;
			}
		}
		if(WithArgCommands == null)
		{
			var Methods = Assembly.GetExecutingAssembly().GetTypes()
				.SelectMany(t => t.GetMethods())
				.Where(m => m.GetCustomAttributes(typeof(CommandWithArg), false).Length > 0);

			WithArgCommands = new List<WithArgCommandInfo>();
			foreach(MethodInfo Method in Methods)
			{
				WithArgCommands.Add(
					new WithArgCommandInfo(
						Method.Name,
						Attribute.GetCustomAttribute(Method, typeof(CommandWithArg)) as CommandWithArg
					)
				);
			}
		}
		foreach(WithArgCommandInfo Method in WithArgCommands)
		{
			if(Method.Name == FunctionName)
			{
				Found = true;
				NewBind.CommandWithArg = Method.Tag.Function;
				break;
			}
		}

		if(!Found)
		{
			Console.ThrowPrint($"The specified function '{FunctionName}' does not exist as a bindable function");
			return false;
		}

		var ButtonValue = ButtonList.Left;
		var AxisDirection = ButtonInfo.DIRECTION.UP;
		var ControllerButtonValue = JoystickList.Axis0;
		uint Scancode = 0;

		//Checks custom string literals first then assumes Scancode
		KeyType kt = null;
		KeyTypes.List.TryGetValue(KeyName, out kt);

		if (kt == null)
		{
			// scancodes
			uint LocalScancode = (uint)OS.FindScancodeFromString(KeyName);
			if(LocalScancode != 0)
			{
				//Is a valid Scancode
				NewBind.Type = ButtonInfo.TYPE.SCANCODE;
				Scancode = LocalScancode;
			}
			else if (KeyName == "`") // this fails on ubuntu 18.04 (scancode of 0 given back)
			{
				NewBind.Type = ButtonInfo.TYPE.SCANCODE;
				Scancode = 96;
			}
			else
			{
				//If not a valid Scancode then the provided key must not be a valid key
				Console.ThrowPrint($"The supplied key '{KeyName}' is not a valid key");
				return false;
			}
		}
		else
		{
			NewBind.Type = kt.Type;
			ButtonValue = kt.ButtonValue;
			AxisDirection = kt.Direction;
			ControllerButtonValue = kt.ControllerButtonValue;
		}

		//Now we have everything we need to setup the bind with Godot's input system

		//First clear any bind with the same key
		UnBind(KeyName);

		//Then add new bind
		if (!InputMap.HasAction(actionName))
		{
			InputMap.AddAction(actionName);
		}
		
		switch(NewBind.Type)
		{
			case(ButtonInfo.TYPE.SCANCODE): {
				InputEventKey Event = new InputEventKey {Scancode = Scancode};
				InputMap.ActionAddEvent(actionName, Event);
				break;
			}

			case(ButtonInfo.TYPE.MOUSEBUTTON):
			case(ButtonInfo.TYPE.MOUSEWHEEL): {
				InputEventMouseButton Event = new InputEventMouseButton {
					ButtonIndex = (int)ButtonValue
				};
				InputMap.ActionAddEvent(actionName, Event);
				break;
			}

			case(ButtonInfo.TYPE.MOUSEAXIS): {
				InputEventMouseMotion Event = new InputEventMouseMotion();
				InputMap.ActionAddEvent(actionName, Event);
				NewBind.AxisDirection = (ButtonInfo.DIRECTION)AxisDirection; //Has to cast as it is Nullable
				break;
			}

			case(ButtonInfo.TYPE.CONTROLLERAXIS): {
				InputEventJoypadMotion Event = new InputEventJoypadMotion {
					Axis = (int)ControllerButtonValue
				};
				// Set which Joystick axis we're using
				switch (AxisDirection) { // Set which direction on the axis we need to trigger the event
					case(ButtonInfo.DIRECTION.UP): {
						Event.AxisValue = -1; // -1, on the Vertical axis is up
						break;
					}

					case(ButtonInfo.DIRECTION.LEFT): {
						Event.AxisValue = -1; // -1, on the Horizontal axis is left
						break;
					}

					case(ButtonInfo.DIRECTION.DOWN): {
						Event.AxisValue = 1; // 1, on the Vertical axis is down
						break;
					}

					case(ButtonInfo.DIRECTION.RIGHT): {
						Event.AxisValue = 1; // 1, on the Horizontal axis is right
						break;
					}
				}

				InputMap.ActionAddEvent(actionName, Event);
				NewBind.AxisDirection = (ButtonInfo.DIRECTION)AxisDirection; //Has to cast as it is Nullable
				break;
			}

			case(ButtonInfo.TYPE.CONTROLLERBUTTON): {
				InputEventJoypadButton Event = new InputEventJoypadButton {
					ButtonIndex = (int) ControllerButtonValue
				};
				InputMap.ActionAddEvent(actionName, Event);
				break;
			}
		}

		if(NewBind.FuncWithArg != null || NewBind.CommandWithArg != null)
		{
			BindingsWithArg.Add(NewBind);
		}
		else if(NewBind.FuncWithoutArg != null)
		{
			BindingsWithoutArg.Add(NewBind);
		}

		return true;
	}


	public static void UnBind(string KeyName)
	{
		KeyName = KeyName.ToLower();
		Godot.Collections.Array actions = InputMap.GetActions();

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
						if (key.ToLower() == KeyName)
						{
							InputMap.ActionEraseEvent(a, iek);

							List<BindingObject> Removing = new List<BindingObject>();
							foreach(BindingObject Bind in BindingsWithArg)
							{
								if(Bind.Name == a)
								{
									Removing.Add(Bind);
								}
							}
							foreach(BindingObject Bind in Removing)
							{
								BindingsWithArg.Remove(Bind);
							}

							Removing.Clear();

							foreach(BindingObject Bind in BindingsWithoutArg)
							{
								if(Bind.Name == KeyName)
								{
									Removing.Add(Bind);
								}
							}
							foreach(BindingObject Bind in Removing)
							{
								BindingsWithoutArg.Remove(Bind);
							}
						}
					}
				}
			}
		}
	}


	private static float GreaterEqualZero(float In)
	{
		if(In < 0f)
		{
			return 0f;
		}
		return In;
	}


	public override void _PhysicsProcess(float Delta)
	{
		// FIXME - change this to use binds system, at least for toggle console?
		//UI handling where action only exists for UI interaction
		if(UIManager.UIOpen())
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				UIManager.UI_Accept();
			}
			else if (Input.IsActionJustPressed("ui_up"))
			{
				UIManager.UI_Up();
			}
			else if (Input.IsActionJustPressed("ui_down"))
			{
				UIManager.UI_Down();
			}
			else if (Input.IsActionJustPressed("ui_cancel"))
			{
				UIManager.UI_Cancel();
			}
			else if (Input.IsActionJustPressed("ui_toggleconsole"))
			{
				UIManager.UI_ConsoleToggle();
			}

			return;
		}

		foreach(BindingObject Binding in BindingsWithArg)
		{
			if (Binding.CommandWithArg != null)
			{
				if (Input.IsActionJustPressed(Binding.Name))
				{
					string arg = Binding.Name.Replace(Binding.CommandWithArg.Method.Name.ToLower(), "").Trim();
					List<string> args = new List<string>{arg};
					Binding.CommandWithArg.Invoke(args);
				}
			}
			else if(Binding.Type == ButtonInfo.TYPE.SCANCODE || Binding.Type == ButtonInfo.TYPE.MOUSEBUTTON || Binding.Type == ButtonInfo.TYPE.CONTROLLERBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Binding.FuncWithArg.Invoke(1);
				}
				else if(Input.IsActionJustReleased(Binding.Name))
				{
					Binding.FuncWithArg.Invoke(-1);
				}
			}
			else if(Binding.Type == ButtonInfo.TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Binding.FuncWithArg.Invoke(1);
				}
			}
			else if(Binding.Type == ButtonInfo.TYPE.CONTROLLERAXIS)
			{
				int VerticalAxis = 0;
				int HorizontalAxis = 0;

				foreach(InputEvent Option in InputMap.GetActionList(Binding.Name)) {
					if (Option is InputEventJoypadMotion JoyEvent) {
						InputEventJoypadMotion StickEvent = JoyEvent;
						if(StickEvent.Axis == 0 || StickEvent.Axis == 1)
						{
							// We are using Left stick
							VerticalAxis = 1;
							HorizontalAxis = 0;
						}
						else if(StickEvent.Axis == 2 || StickEvent.Axis == 3)
						{
							// We are using Right stick
							VerticalAxis = 3;
							HorizontalAxis = 2;
						}
						else
						{
							Console.ThrowPrint("This joystick doesn't seem to exist");
							return;
						}
					}
				}

				if (Math.Abs(Input.GetJoyAxis(0,HorizontalAxis)) >= Settings.Deadzone || Math.Abs(Input.GetJoyAxis(0,VerticalAxis)) >= Settings.Deadzone)
				{
					float HorizontalMovement = Input.GetJoyAxis(0,HorizontalAxis);
					float VerticalMovement = Input.GetJoyAxis(0,VerticalAxis);
					switch(Binding.AxisDirection)
					{
						case(ButtonInfo.DIRECTION.UP):
							Binding.FuncWithArg.Invoke(-VerticalMovement);
							break;
						case(ButtonInfo.DIRECTION.DOWN):
							Binding.FuncWithArg.Invoke(VerticalMovement);
							break;
						case(ButtonInfo.DIRECTION.RIGHT):
							Binding.FuncWithArg.Invoke(HorizontalMovement);
							break;
						case(ButtonInfo.DIRECTION.LEFT):
							Binding.FuncWithArg.Invoke(-HorizontalMovement);
							break;
					}
					Binding.JoyWasInDeadzone = false;
				}
				else // Set sens to zero to simulate key release
				{
					if (Binding.JoyWasInDeadzone == false) // Only do this if the Binding wasn't zero last time
					{
						Binding.FuncWithArg.Invoke(0);
						Binding.JoyWasInDeadzone = true;
					}
				}
			}
		}

		foreach(BindingObject Binding in BindingsWithoutArg)
		{
			if(Binding.Type == ButtonInfo.TYPE.SCANCODE || Binding.Type == ButtonInfo.TYPE.MOUSEBUTTON || Binding.Type == ButtonInfo.TYPE.CONTROLLERBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Binding.FuncWithoutArg.Invoke();
				}
			}
			else if(Binding.Type == ButtonInfo.TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Binding.FuncWithoutArg.Invoke();
				}
			}
		}
	}

	public override void _Input(InputEvent Event)
	{
		if(Event is InputEventMouseMotion MotionEvent)
		{
			if (Input.GetMouseMode() == Input.MouseMode.Captured)
			{
				foreach(BindingObject Binding in BindingsWithArg)
				{
					if(Binding.Type == ButtonInfo.TYPE.MOUSEAXIS)
					{
						switch(Binding.AxisDirection)
						{
							case(ButtonInfo.DIRECTION.UP):
								Binding.FuncWithArg.Invoke(GreaterEqualZero(-MotionEvent.Relative.y));
								break;
							case(ButtonInfo.DIRECTION.DOWN):
								Binding.FuncWithArg.Invoke(GreaterEqualZero(MotionEvent.Relative.y));
								break;
							case(ButtonInfo.DIRECTION.RIGHT):
								Binding.FuncWithArg.Invoke(GreaterEqualZero(MotionEvent.Relative.x));
								break;
							case(ButtonInfo.DIRECTION.LEFT):
								Binding.FuncWithArg.Invoke(GreaterEqualZero(-MotionEvent.Relative.x));
								break;
						}
					}
				}

				foreach(BindingObject Binding in BindingsWithoutArg)
				{
					if(Binding.Type == ButtonInfo.TYPE.MOUSEAXIS)
					{
						//Don't need to switch on the direction as it doesn't want an argument anyway
						Binding.FuncWithoutArg.Invoke();
					}
				}
			}
		}
	}
}
