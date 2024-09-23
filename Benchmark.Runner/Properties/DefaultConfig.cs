using System.Collections.Immutable;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmark.Runner.Properties;

public class DefaultConfig : ManualConfig
{
	public DefaultConfig()
	{
		var defaultJob = Job.Default;
		AddJob(defaultJob.WithRuntime(CoreRuntime.Core80));
		AddJob(defaultJob.WithRuntime(NativeAotRuntime.Net80));
		AddLogger(ConsoleLogger.Default);
		AddExporter(MarkdownExporter.Default);
		AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(false)));
		var instance = BenchmarkDotNet.Configs.DefaultConfig.Instance;
		AddAnalyser(
			instance.GetAnalysers()
					.ToArray());
		AddDiagnoser(
			instance.GetDiagnosers()
					.ToArray());
		AddColumn(JobCharacteristicColumn.AllColumns);
		AddColumn(StatisticColumn.Mean);
		AddColumn(RatioColumn.Default);
		AddColumn(HashColumn.Default);
		HideColumns(StatisticColumn.Error, StatisticColumn.StdDev, StatisticColumn.Median);
		HideColumns(TargetMethodColumn.Method);
		HideColumns(nameof(Runner.EntityCount), nameof(Runner.Ticks));
		Options |= ConfigOptions.DontOverwriteResults;
		Orderer =  new DefaultOrderer();
	}

	private class DefaultOrderer : IOrderer
	{
		public bool SeparateLogicalGroups
		{
			get => true;
		}

		public IEnumerable<BenchmarkCase> GetExecutionOrder(
			ImmutableArray<BenchmarkCase> benchmarksCases,
			IEnumerable<BenchmarkLogicalGroupRule>? order = null) =>
			benchmarksCases.OrderBy(benchmarkCase => benchmarkCase.Job.Environment.Runtime.RuntimeMoniker)
						   .ThenBy(benchmarkCase => benchmarkCase.Parameters, ParameterComparer.Instance);

		public IEnumerable<BenchmarkCase> GetSummaryOrder(
			ImmutableArray<BenchmarkCase> benchmarksCases,
			Summary summary) =>
			benchmarksCases.OrderBy(benchmarkCase => benchmarkCase.Job.Environment.Runtime.RuntimeMoniker)
						   .ThenBy(benchmarkCase => benchmarkCase.Parameters, ParameterComparer.Instance)
						   .ThenBy(benchmarksCase => summary[benchmarksCase]?.ResultStatistics?.Mean ?? 0);

		public string? GetHighlightGroupKey(BenchmarkCase benchmarkCase) =>
			null;

		public string GetLogicalGroupKey(ImmutableArray<BenchmarkCase> allBenchmarksCases, BenchmarkCase benchmarkCase)
		{
			var sb = new StringBuilder();
			sb.Append(benchmarkCase.Job.DisplayInfo);
			foreach (var item in benchmarkCase.Parameters.Items)
			{
				var equatable = item.Definition.ParameterType.GetInterfaces()
									.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEquatable<>));
				if (equatable)
					sb.Append(item);
			}

			return sb.ToString();
		}

		public IEnumerable<IGrouping<string, BenchmarkCase>> GetLogicalGroupOrder(
			IEnumerable<IGrouping<string, BenchmarkCase>> logicalGroups,
			IEnumerable<BenchmarkLogicalGroupRule>? order = null) =>
			logicalGroups.OrderBy(it => it.Key);
	}

	private class ParameterComparer : IComparer<ParameterInstances>
	{
		public static readonly ParameterComparer Instance = new();

		public int Compare(ParameterInstances? x, ParameterInstances? y)
		{
			if (x == null
			 && y == null)
				return 0;
			if (x != null && y == null
			 || y == null)
				return 1;
			if (x == null)
				return -1;

			for (var i = 0; i < Math.Min(x.Count, y.Count); i++)
			{
				var compareTo = CompareValues(x[i].Value, y[i].Value);
				if (compareTo != 0)
					return compareTo;
			}

			return 0;
		}

		private static int CompareValues(object? x, object? y) =>
			x != null && y != null && x.GetType() == y.GetType() && x is IComparable xComparable
				? xComparable.CompareTo(y)
				: 0;
	}

	private class RatioColumn : IColumn
	{
		public static readonly IColumn Default = new RatioColumn();
		private RatioColumn() {}

		public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
		{
			var logicalGroup = summary.GetLogicalGroupForBenchmark(benchmarkCase)
									  .Where(
										   b =>
										   {
											   var statistics = summary[b]?.ResultStatistics;
											   if (statistics != null)
												   return statistics.Mean != 0.0;
											   return false;
										   })
									  .Select(b => (benchmarkCase: b, mean: summary[b]!.ResultStatistics!.Mean))
									  .ToArray();
			var result = summary[benchmarkCase]?.ResultStatistics?.Mean;
			if (result == null)
				return "?";
			var min   = logicalGroup.MinBy(b => b.mean);
			var ratio = result.Value / min.mean;

			return benchmarkCase == min.benchmarkCase ? "min" :
				ratio            >= 1.0 ? "+" + ((ratio - 1.0) * 100).ToString("N1", summary.GetCultureInfo()) + "%" :
										  "-" + ((1.0 - ratio) * 100).ToString("N1", summary.GetCultureInfo()) + "%";
		}

		public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
			GetValue(summary, benchmarkCase);

		public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) =>
			false;

		public bool IsAvailable(Summary summary) =>
			true;

		public string Id
		{
			get => nameof(RatioColumn);
		}

		public string ColumnName
		{
			get => Column.Ratio;
		}

		public bool AlwaysShow
		{
			get => true;
		}

		public ColumnCategory Category
		{
			get => ColumnCategory.Custom;
		}

		public int PriorityInCategory
		{
			get => 0;
		}

		public bool IsNumeric
		{
			get => true;
		}

		public UnitType UnitType
		{
			get => UnitType.Dimensionless;
		}

		public string Legend
		{
			get => "Mean of the ratio distribution ([Current]/[Min])";
		}

		public override string ToString() =>
			ColumnName;
	}

	private class HashColumn : IColumn
	{
		public static readonly IColumn Default = new HashColumn();
		private HashColumn() {}

		public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
		{
			foreach (var report in summary.Reports)
			{
				if (report.BenchmarkCase != benchmarkCase)
					continue;

				var hashes = GetHashesFromOutput(report, "// Hash: ");
				if (hashes.Length == 0)
					break;

				var first = hashes[0];
				return hashes.Any(h => h != first) ? $"!{first}" : $" {first}";
			}

			return "?";
		}

		private static string[] GetHashesFromOutput(BenchmarkReport report, string prefix)
		{
			return (
				from executeResults in report.ExecuteResults
				from extraOutputLine in executeResults.StandardOutput.Where(line => line.StartsWith(prefix))
				select extraOutputLine[prefix.Length..]).ToArray();
		}

		public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
			GetValue(summary, benchmarkCase);

		public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) =>
			false;

		public bool IsAvailable(Summary summary) =>
			true;

		public string Id
		{
			get => nameof(HashColumn);
		}

		public string ColumnName
		{
			get => "Hash";
		}

		public bool AlwaysShow
		{
			get => false;
		}

		public ColumnCategory Category
		{
			get => ColumnCategory.Custom;
		}

		public int PriorityInCategory
		{
			get => 0;
		}

		public bool IsNumeric
		{
			get => false;
		}

		public UnitType UnitType
		{
			get => UnitType.Dimensionless;
		}

		public string Legend
		{
			get => "Hash";
		}

		public override string ToString() =>
			ColumnName;
	}
}
