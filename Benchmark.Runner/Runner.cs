using Benchmark.Core;
using Benchmark.Core.Hash;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Runner;

public class Runner
{
	private Framebuffer _framebuffer;
	private uint[]      _hashes = default!;
	private uint        _hash;

	public static IEnumerable<IContext> Contexts
	{
		get => Benchmark.Contexts.Factories.Select(tuple => tuple.factory());
	}

	[ParamsSource(nameof(Contexts))]
	public IContext? Context { get; set; }

	[Params(16384)]
	public int EntityCount { get; set; }

	[Params(1024)]
	public int Ticks { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		_hashes = new uint[Ticks];
	}

	[IterationSetup]
	public void Setup()
	{
		_framebuffer = new Framebuffer(Common.FrameBufferWidth, Common.FrameBufferHeight, EntityCount * Ticks);
		Array.Clear(_hashes, 0, _hashes.Length);
	}

	[Benchmark]
	public void Run()
	{
		if (Context is not {} context)
			return;
		context.Setup(EntityCount, _framebuffer);
		for (var i = 0; i < Ticks; i++)
		{
			context.Step(i);
			_hashes[i] = StableHash32.Hash(0, context.Framebuffer.BufferSpan);
		}
		context.Cleanup();
	}

	[IterationCleanup]
	public void Cleanup()
	{
		_framebuffer.Dispose();
		_framebuffer = default;
		_hash        = StableHash32.Hash(0, _hashes);
	}

	[GlobalCleanup]
	public void GlobalCleanup() =>
		Console.WriteLine("// Hash: {0:X8}", _hash);
}
