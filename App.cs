using Photon.Deterministic;
using Quantum.CodeGen;

namespace QtmCodegenStandalone;

public class App
{
	private const string QTN_OUT_PATH = "QuantumUser/Simulation/Generated";
	private const string UNITY_OUT_PATH = "QuantumUser/View/Generated";

	public static void Generate(string projectPath)
	{
		Assert.Check(!string.IsNullOrEmpty(projectPath));

		if (!Directory.Exists(projectPath))
		{
			Console.Error.WriteLine($"Non-existant project path provided: \"{projectPath}\"");
			return;
		}

		string assetsPath = projectPath;
		if (!assetsPath.EndsWith("Assets"))
		{
			assetsPath = Path.Join(assetsPath, "Assets");

			if (!Directory.Exists(assetsPath))
			{
				Console.Error.WriteLine($"Cannot find assets folder for project at path \"{projectPath}\"");
				return;
			}
		}

		var assets = new List<string>();
		try
		{
			assets.AddRange(
				Directory.EnumerateFiles(
					assetsPath,
					"*.qtn",
					SearchOption.AllDirectories));
		}
		catch (Exception e)
		{
			Console.Error.WriteLine($"Failed to search assets folder: {e}");
			return;
		}

		Console.WriteLine($"Found {assets.Count} qtn files, running codegen...");

		var outputFiles = new List<GeneratorOutputFile>();
		try
		{
			outputFiles.AddRange(
				Generator.Generate(
					assets,
					new GeneratorOptions(),
					warning =>
					{
						string msg = "";
						if (!string.IsNullOrEmpty(warning.Path))
						{
							msg += $"{warning.Path}({warning.Position}): ";
						}

						msg += warning.Message;

						if (!string.IsNullOrEmpty(warning.SourceCode))
						{
							msg += $"\n{warning.SourceCode}";
						}
						Console.WriteLine($"[codegen] {msg}");
					}));
		}
		catch (Exception e)
		{
			Console.Error.WriteLine($"Generation failed: {e}");
			return;
		}

		Console.WriteLine($"Successfully generated code for {outputFiles.Count} files, writing to disk...");

		ILookup<bool, GeneratorOutputFile> grouped = outputFiles.ToLookup(
			file =>
			{
				switch (file.Kind)
				{
					case GeneratorOutputFileKind.UnityPrototypeAdapters:
					case GeneratorOutputFileKind.UnityPrototypeWrapper:
					case GeneratorOutputFileKind.UnityLegacyAssetBase:
						return true;
					default:
						return false;
				}
			});
		List<GeneratorOutputFile> qtmFiles = grouped[false].ToList();
		List<GeneratorOutputFile> unityFiles = grouped[true].ToList();

		WriteOutputToDirectory(Path.Combine(assetsPath, QTN_OUT_PATH), qtmFiles);
		WriteOutputToDirectory(Path.Combine(assetsPath, UNITY_OUT_PATH), unityFiles);

		Console.WriteLine($"Successfully wrote {outputFiles.Count} generated files to disk");
	}

	private static void WriteOutputToDirectory(
		string outputDir,
		IReadOnlyCollection<GeneratorOutputFile> files)
	{
		Console.WriteLine($"Writing {files.Count} generated scripts to \"{outputDir}\"");

		var fileNames = new HashSet<string>();
		foreach (GeneratorOutputFile file in files)
		{
			if (string.IsNullOrEmpty(file.Contents))
			{
				continue;
			}

			if (string.IsNullOrEmpty(file.UserFolder) && !fileNames.Add(file.Name))
			{
				Console.Error.WriteLine($"Duplicate output for file \"{file.Name}\"");
				continue;
			}

			string outPath = Path.Combine(
				string.IsNullOrEmpty(file.UserFolder) ? outputDir : file.UserFolder,
				file.Name);

			if (file.FormerNames?.Length > 0 && !File.Exists(outPath))
			{
				foreach (string formerName in file.FormerNames)
				{
					string formerPath = Path.Combine(outputDir, formerName);
					if (File.Exists(formerPath))
					{
						File.Move(formerPath, outPath);
						string formerMetaPath = $"{formerPath}.meta";
						if (File.Exists(formerMetaPath))
						{
							string outMetaPath = $"{outPath}.meta";
							File.Move(formerMetaPath, outMetaPath);
						}

						break;
					}
				}
			}

			if (!IsOutputStale(outPath, file.Contents))
			{
				continue;
			}

			File.WriteAllText(outPath, file.Contents);
		}
	}

	private static bool IsOutputStale(string path, string newContents)
	{
		if (!File.Exists(path))
		{
			return true;
		}

		string oldContents = File.ReadAllText(path);
		if (oldContents.Length != newContents.Length)
		{
			return true;
		}

		ulong checksumOld = CRC64.Calculate(0, oldContents);
		ulong checksumNew = CRC64.Calculate(0, newContents);
		return checksumOld != checksumNew;
	}
}
