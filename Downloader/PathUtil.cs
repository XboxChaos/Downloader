using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader
{
	/// <summary>
	/// Contains utility functions for working with paths.
	/// </summary>
	static class PathUtil
	{
		// http://stackoverflow.com/a/479198
		public static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
		{
			var parentDirInfo = dirInfo.Parent;
			if (null == parentDirInfo)
				return dirInfo.Name.Substring(0, 1).ToUpperInvariant() + dirInfo.Name.Substring(1); // Fix drive letter capitalization
			return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
				parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
		}

		public static string GetProperDirectoryCapitalization(string dirPath)
		{
			return GetProperDirectoryCapitalization(new DirectoryInfo(dirPath));
		}

		/// <summary>
		/// Determines whether or not a directory path contains valid characters.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns><c>true</c> if the path is valid.</returns>
		public static bool IsDirectoryPathValid(string path)
		{
			try
			{
				// The DirectoryInfo constructor will throw an exception if the path is invalid
				var info = new DirectoryInfo(path);
				return true;
			}
			catch (Exception)
			{
			}
			return false;
		}
	}
}
