using System.Threading;

namespace PowerOfTenAnalyzers.Sample.PT0201;

public class AcceptableInfiniteLoops
{
	public static SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(0);
	public static void AlwaysConsume()
	{
		while (true)
			Semaphore.Wait();
		// ReSharper disable once FunctionNeverReturns
	}

	public static void AlwaysConsumeConst()
	{
		const bool constant = true;
		while (constant)
		{
			Semaphore.Wait();
		}
		// ReSharper disable once FunctionNeverReturns
	}
}