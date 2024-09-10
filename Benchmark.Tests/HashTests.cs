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
		uint?          hash = default;
		StringBuilder? sb   = null;
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
			catch (AssertionException assert)
			{
				sb ??= new StringBuilder();
				sb.AppendLine();
				sb.AppendLine(assert.Message);
			}
			catch (Exception e)
			{
				sb ??= new StringBuilder();
				sb.AppendLine();
				sb.AppendLine(context.ToString());
				sb.AppendLine(e.Message);
				sb.AppendLine(e.StackTrace);
			}

		if (sb is not null)
			Assert.Fail(sb.ToString());
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
					hashCode.Add(framebuffer.Buffer);
			}

			TestContext.Out.WriteLine(context);
			if (log)
			{
				var sb = new StringBuilder();
				foreach (var (tick, id, x, y, c) in framebuffer.Draws)
					sb.Append($"{tick:0000},{id},{x},{y},{c}\n");
				TestContext.Out.Write(sb);
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
