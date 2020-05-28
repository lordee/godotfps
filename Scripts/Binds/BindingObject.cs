using System;
using System.Collections.Generic;


public class BindingObject
{
	public string Name = null; //Null to fail early
	public string Key = null;
	public Action FuncWithoutArg = null;
	public Action<float> FuncWithArg = null;
	public Action<List<string>> CommandWithArg = null;
	public ButtonInfo.TYPE Type = ButtonInfo.TYPE.UNSET;
	public ButtonInfo.DIRECTION AxisDirection; //Only used if Type is AXIS

	public bool JoyWasInDeadzone = true;

	public BindingObject(string name, string key)
	{
		Name = name;
		Key = key;
	}

	public bool Equals(BindingObject Other)
	{
		return Name == Other.Name;
	}
}
