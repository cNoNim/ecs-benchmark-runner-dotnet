using System.Text;
using Benchmark.Core;
using Benchmark.Core.Hash;

namespace Benchmark.Tests;

public partial class HashTests
{
	[TestCase(1,    1)]
	[TestCase(2,    4)]
	[TestCase(4,    16)]
	[TestCase(8,    64)]
	[TestCase(256,  16)]
	[TestCase(2048, 32)]
	public void Check(int entityCount, int ticks) =>
		TestContexts(
			Contexts.Factories.Select(factory => factory.factory()),
			entityCount,
			ticks,
			true,
			true);

	[TestCase(16, 256)]
	[TestCase(32, 1024)]
	public void CheckLong(int entityCount, int ticks) =>
		TestContexts(
			Contexts.Factories.Select(factory => factory.factory()),
			entityCount,
			ticks,
			true,
			false);

	private static void TestContexts(
		IEnumerable<IContext> contexts,
		int entityCount,
		int ticks,
		bool checkHash,
		bool log)
	{
		uint?            hash       = default;
		List<Exception>? exceptions = null;
		foreach (var context in contexts)
			try
			{
				Test(
					context,
					ref hash,
					entityCount,
					ticks,
					checkHash,
					log);
			}
			catch (AssertionException) {}
			catch (Exception e)
			{
				exceptions ??= [];
				exceptions.Add(e);
			}

		if (exceptions != null)
			throw new AggregateException(exceptions);
	}

	private static void Test(
		IContext context,
		ref uint? hash,
		int entityCount,
		int ticks,
		bool checkHash,
		bool log)
	{
		using var framebuffer = new Framebuffer(
			Common.FrameBufferWidth,
			Common.FrameBufferHeight,
			log ? entityCount * ticks : 0);
		context.Setup(entityCount, framebuffer);
		try
		{
			var hashCode = new StableHashCode();
			for (var i = 0; i < ticks; i++)
			{
				context.Step(i);
				if (checkHash)
					hashCode.Add(framebuffer.BufferSpan);
			}

			if (log)
			{
				var draws = new List<string>();
				foreach (var (tick, id, x, y, c) in framebuffer.DrawsSpan)
					draws.Add($"{tick:0000},{id},{x},{y},{c}");
				draws.Sort();
				var sb = new StringBuilder();
				foreach (var draw in draws)
					sb.AppendLine(draw);
				Directory.CreateDirectory("TestResults");
				var dumpPath = $"TestResults/{context}_{entityCount}_{ticks}.dump";
				File.WriteAllText(dumpPath, sb.ToString());
				TestContext.AddTestAttachment(dumpPath, context.ToString());
#if DEBUG
				if (context is ILogs logs)
				{
					sb.Clear();
					foreach (var (stamp, message) in logs.Logs)
						sb.AppendLine($"{stamp}: {message}");
					if (sb.Length > 0)
					{
						var logsPath = $"TestResults/{context}_{entityCount}_{ticks}.log";
						File.WriteAllText(logsPath, sb.ToString());
						TestContext.AddTestAttachment(logsPath, context.ToString());
					}
				}
#endif
			}
			if (!checkHash)
				return;

			var newHash = (uint) hashCode.ToHashCode();
			if (hash != null)
				Assert.That(newHash, Is.EqualTo(hash), context.ToString());
			hash = newHash;
		}
		finally
		{
			context.Cleanup();
		}
	}
}
