namespace QtmCodegenStandalone;

class Program
{
	private static void Main(string[] args)
	{
		if (args.Length != 1)
		{
			Console.Error.WriteLine($"Invalid number of arguments, expected 1 received {args.Length}");
			PrintHelp();
			return;
		}

		string projectPath = args[0];
		if (!Directory.Exists(projectPath))
		{
			Console.Error.WriteLine($"Non-existent project path \"{projectPath}\"");
			PrintHelp();
			return;
		}

		App.Generate(projectPath);
	}

	private static void PrintHelp()
	{
		Console.WriteLine("Usage: QtmCodegenStandalone.exe path/to/project/root");
	}
}
