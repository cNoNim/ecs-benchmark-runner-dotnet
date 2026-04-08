using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;

namespace Benchmark.Runner;

public static class Program
{
	public static void Main(string[] args)
	{
		var (runnerOptions, benchmarkArgs) = RunnerArguments.Parse(args);
		RunnerArguments.ValidateContextNames(runnerOptions, Runner.Contexts);

		var config = new BenchmarkConfig();
		var contextFilter = new SimpleFilter(
			benchmarkCase =>
			{
				var parameter = benchmarkCase.Parameters.Items.FirstOrDefault(item => item.Name == nameof(Runner.Context));
				if (parameter?.Value is not {} context)
					return true;

				var contextName = RunnerArguments.Normalize(context.ToString() ?? string.Empty);
				if (runnerOptions.Contexts.Count != 0 && !runnerOptions.Contexts.Contains(contextName))
					return false;

				return !runnerOptions.ContextExcludes.Contains(contextName);
			});
		config.AddFilter(contextFilter);

		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
						 .Run(benchmarkArgs, config);
	}
}
