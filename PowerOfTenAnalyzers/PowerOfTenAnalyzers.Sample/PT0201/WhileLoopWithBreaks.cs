using System;

namespace PowerOfTenAnalyzers.Sample.PT0201;

public class WhileLoopWithBreaks
{
	public void Method()
	{
		while (true)
		{
			var entered = Console.ReadLine();

			if (string.IsNullOrEmpty(entered))
			{
				break;
			}

			var spaceIndex = -1;
			var index = 0;
			foreach (var character in entered)
			{
				if (character == ' ')
				{
					spaceIndex = index;
					break;
				}
			}
			
			Console.WriteLine($"{spaceIndex}: {entered}");

			if (spaceIndex == -1)
			{
				return;
			}
		}
	}
}