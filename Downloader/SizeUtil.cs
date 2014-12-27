using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader
{
	/// <summary>
	/// Contains methods for formatting file sizes for display.
	/// </summary>
	static class SizeUtil
	{
		private static readonly string[] Units = new string[] {"B", "KB", "MB", "GB", "TB", "PB", "EB"};

		/// <summary>
		/// Formats a file size for display.
		/// </summary>
		/// <param name="size">The size in bytes.</param>
		/// <returns>A string representing the file size, which can be shown to a user.</returns>
		public static string FormatSizeForDisplay(long size)
		{
			if (size <= 0)
				return "0 " + Units[0];

			// Keep dividing the size by 1024 until it becomes too small,
			// incrementing the power to display each time
			var power = 0;
			var convertedSize = (double) size;
			while (convertedSize >= 1024 && power < Units.Length - 1)
			{
				convertedSize /= 1024;
				power++;
			}
			return Math.Round(convertedSize, 1) + " " + Units[power];
		}

		/// <summary>
		/// Formats a transfer rate for display.
		/// </summary>
		/// <param name="size">The number of bytes downloaded.</param>
		/// <param name="timeDelta">The amount of time in seconds it took to download the given number of bytes.</param>
		/// <returns>A string representing the transfer rate, which can be shown to a user.</returns>
		public static string FormatRateForDisplay(long size, double timeDelta)
		{
			var scaledSize = size / timeDelta;
			return FormatSizeForDisplay((long) scaledSize) + "/s";
		}
	}
}
