using System;

using Xamarin.Forms;

namespace XCameraSample.Utils
{
	public static class MathUtils
	{
		public static Size FitSize4X3(double width, double height)
		{
			var smallest = Math.Min(width, height);
			double ratio = 4d / 3d;

			if (smallest == width)
			{
				return new Size(smallest, smallest * ratio);
			}

			else
			{
				return new Size(smallest * ratio, smallest);
			}
		}
	}
}

