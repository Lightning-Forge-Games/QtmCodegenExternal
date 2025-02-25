using System.Diagnostics;

namespace QtmCodegenStandalone;

public static class Assert
{
	public static void Check(bool condition, string? message = null)
	{
		Debug.Assert(condition, message);
	}
}
