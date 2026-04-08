using System.Text;
using Benchmark.Core;

namespace Benchmark.Runner;

public static class RunnerArguments
{
	public static (RunnerOptions options, string[] benchmarkArgs) Parse(string[] args)
	{
		var benchmarkArgs = new List<string>();
		var contexts = new HashSet<string>(StringComparer.Ordinal);
		var excludes = new HashSet<string>(StringComparer.Ordinal);

		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];
			if (TryReadOption(arg, "--contexts", out var includeValue))
			{
				AddValues(contexts, includeValue, args, ref i, "--contexts");
				continue;
			}

			if (TryReadOption(arg, "--context-excludes", out var excludeValue))
			{
				AddValues(excludes, excludeValue, args, ref i, "--context-excludes");
				continue;
			}

			benchmarkArgs.Add(arg);
		}

		return (new RunnerOptions(contexts, excludes), benchmarkArgs.ToArray());
	}

	public static void ValidateContextNames(RunnerOptions options, IEnumerable<IContext> contexts)
	{
		if (options.Contexts.Count == 0 && options.ContextExcludes.Count == 0)
			return;

		var available = contexts.Select(context => context.ToString() ?? context.GetType().Name)
								.Distinct(StringComparer.Ordinal)
								.ToArray();
		var availableNormalized = available.Select(Normalize)
										   .ToHashSet(StringComparer.Ordinal);
		var unknown = options.Contexts.Concat(options.ContextExcludes)
								 .Distinct(StringComparer.Ordinal)
								 .Where(name => !availableNormalized.Contains(name))
								 .ToArray();
		if (unknown.Length == 0)
			return;

		throw new ArgumentException(
			$"Unknown context name(s): {string.Join(", ", unknown)}. " +
			$"Available contexts: {string.Join(", ", available)}");
	}

	public static string Normalize(string value)
	{
		var sb = new StringBuilder(value.Length);
		foreach (var c in value.Where(char.IsLetterOrDigit))
			sb.Append(char.ToLowerInvariant(c));

		return sb.ToString();
	}

	private static bool TryReadOption(string arg, string optionName, out string? value)
	{
		if (arg == optionName)
		{
			value = null;
			return true;
		}

		var prefix = optionName + "=";
		if (arg.StartsWith(prefix, StringComparison.Ordinal))
		{
			value = arg[prefix.Length..];
			return true;
		}

		value = null;
		return false;
	}

	private static void AddValues(HashSet<string> target, string? inlineValue, string[] args, ref int index, string optionName)
	{
		var value = inlineValue;
		if (value == null)
		{
			if (index + 1 >= args.Length)
				throw new ArgumentException($"Missing value for {optionName}.");
			value = args[++index];
		}

		foreach (var item in value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
		{
			var normalized = Normalize(item);
			if (normalized.Length != 0)
				target.Add(normalized);
		}
	}
}

public sealed record RunnerOptions(
	HashSet<string> Contexts,
	HashSet<string> ContextExcludes);
