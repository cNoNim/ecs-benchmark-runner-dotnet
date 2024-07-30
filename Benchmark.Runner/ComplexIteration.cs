using Benchmark.Core;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Runner;

public class ComplexIteration
{
	private Framebuffer _framebuffer;

	public static IEnumerable<IContext> Contexts
	{
		get => Benchmark.Contexts.Factories.Select(tuple => tuple.factory());
	}

	[ParamsSource(nameof(Contexts))]
	public IContext? Context { get; set; }

	[Params(2048)]
	public int EntityCount { get; set; }

	[Params(1024)]
	public int Ticks { get; set; }

	[IterationSetup]
	public void Setup()
	{
		_framebuffer = new Framebuffer(Common.FrameBufferWidth, Common.FrameBufferHeight, EntityCount * Ticks);
		Context?.Setup(EntityCount, _framebuffer);
	}

	[Benchmark]
	public void Run()
	{
		if (Context is not {} context)
			return;
		for (var i = 0; i < Ticks; i++)
			context.Step(i);
	}

	[IterationCleanup]
	public void Cleanup()
	{
		Context?.Cleanup();
		_framebuffer.Dispose();
		_framebuffer = default;
	}
}
