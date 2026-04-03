using System.Runtime.InteropServices;
using Benchmark.Core.Hash;

namespace Benchmark.Tests;

public class XxHash32Tests
{
	[Test]
	public void XxHash32([Range(0, 16)] int count)
	{
		Span<uint> span = stackalloc uint[count];

		uint i = 0;
		foreach (ref var v in span)
			v = i++;

		Assert.That(
			StableHash32.Hash(0, span),
			Is.EqualTo(System.IO.Hashing.XxHash32.HashToUInt32(MemoryMarshal.AsBytes(span))));
	}
}
