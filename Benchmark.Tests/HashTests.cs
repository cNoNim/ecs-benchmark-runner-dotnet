using System.Text;
using Benchmark.Core;
using Benchmark.Core.Hash;

namespace Benchmark.Tests;

public class HashTests
{
	[TestCase(1,    1)]
	[TestCase(2,    4)]
	[TestCase(4,    16)]
	[TestCase(8,    64)]
	[TestCase(256,  16)]
	[TestCase(2048, 32)]
	public void Check(int entityCount, int ticks) =>
		TestContexts(Contexts.Factories.Select(factory => factory.factory()), entityCount, ticks);

	[TestCase(16, 256)]
	[TestCase(32, 1024)]
	public void CheckLong(int entityCount, int ticks) =>
		Check(entityCount, ticks);

	private static void TestContexts(IEnumerable<IContext> contexts, int entityCount, int ticks)
	{
		uint? hash = default;
		var   fail = false;
		var   sb   = new StringBuilder();
		sb.AppendLine();
		foreach (var context in contexts)
			try
			{
				Test(
					context,
					ref hash,
					entityCount,
					ticks);
			}
			catch (AssertionException assert)
			{
				fail = true;
				sb.AppendLine(assert.Message);
			}

		if (fail)
			Assert.Fail(sb.ToString());
	}

	private static void Test(
		IContext context,
		ref uint? hash,
		int entityCount,
		int ticks)
	{
		using var framebuffer = new Framebuffer(Common.FrameBufferWidth, Common.FrameBufferHeight, entityCount * ticks);
		context.Setup(entityCount, framebuffer);
		try
		{
			var hashCode = new StableHashCode();
			for (var i = 0; i < ticks; i++)
			{
				context.Step(i);
				hashCode.Add(framebuffer.Buffer);
			}

			TestContext.Out.WriteLine(context.ToString());
			var sb = new StringBuilder();
			foreach (var (tick, id, x, y, c) in framebuffer.Draws)
				sb.Append($"{tick:0000},{id},{x},{y},{c}\n");
			TestContext.Out.Write(sb.ToString());
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
