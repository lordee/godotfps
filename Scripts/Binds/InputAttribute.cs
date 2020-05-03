using System;
using System.Reflection;

public class InputWithoutArg : Attribute
{
	public Action Function;


	public InputWithoutArg(Type T, string FunctionName)
	{
		MethodInfo Method = T.GetMethod(FunctionName);
		if(!Method.IsStatic)
			throw new Exception($"Method {FunctionName} is not static");
		if(Method.GetParameters().Length > 0)
			throw new Exception($"Method {FunctionName} has arguments");

		Function = (Action)Delegate.CreateDelegate(typeof(Action), Method);
	}
}

public class InputWithArg : Attribute
{
	public Action<float> Function;


	public InputWithArg(Type T, string FunctionName)
	{
		MethodInfo Method = T.GetMethod(FunctionName);
		if(!Method.IsStatic)
			throw new Exception($"Method {FunctionName} is not static");

		Function = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), Method);
	}
}
