using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XboxChaos.Models;

namespace Downloader.ViewModels
{
	/// <summary>
	/// Holds settings describing how the application should be installed.
	/// </summary>
	[Export(typeof(InstallSettings))]
	class InstallSettings
	{
		public InstallSettings()
		{
			BranchName = "master"; // Default to the master branch
			InstallFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // Default to the assembly directory
		}

		/// <summary>
		/// Gets or sets the application info to use.
		/// </summary>
		public ApplicationResponse ApplicationInfo { get; set; }

		/// <summary>
		/// Gets or sets the name of the branch that should be installed.
		/// </summary>
		public string BranchName { get; set; }

		/// <summary>
		/// Gets or sets the path to the folder that the application should be installed to.
		/// </summary>
		public string InstallFolder { get; set; }
	}
}
