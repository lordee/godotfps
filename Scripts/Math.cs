using Godot;
using System;
using static Godot.Mathf;

public static class FMath
{

    public static Vector3 Flattened(this Vector3 Self)
	{
		return new Vector3(Self.x, 0, Self.z);
	}

    public static Vector3 ClampVec3(Vector3 Vec, float Min, float Max)
	{
		return Vec.Normalized() * Mathf.Clamp(Vec.Length(), Min, Max);
	}

    public static float LoopRotation(float Rot)
	{
		Rot = Rot % 360;

		if(Rot < 0)
			Rot += 360;

		if(Rot == 360)
			Rot = 0;

		return Rot;
	}
}