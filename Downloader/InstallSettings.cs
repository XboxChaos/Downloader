﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XboxChaos.Models;

namespace Downloader
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
			TemporaryFiles = new TempFileCollection();
			RunOnFinish = true;

			// Default the install folder to the working directory
			InstallFolder = Directory.GetCurrentDirectory();
			try
			{
				// Attempt to clean up capitalization
				InstallFolder = PathUtil.GetProperDirectoryCapitalization(InstallFolder);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Gets the path to the application's extracted executable.
		/// </summary>
		/// <returns>The path to the application's extracted executable.</returns>
		public string GetApplicationExePath()
		{
			// Assume there's an executable in the install directory named after the application
			return Path.Combine(InstallFolder, ApplicationInfo.Name + ".exe");
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

		/// <summary>
		/// Gets or sets the path to the application's zip file after it has been downloaded.
		/// </summary>
		public string ApplicationZipPath { get; set; }

		/// <summary>
		/// Gets or sets the path to the updater's zip file after it has been downloaded.
		/// </summary>
		public string UpdateZipPath { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the application should be run when the downloader is finished.
		/// </summary>
		public bool RunOnFinish { get; set; }

		/// <summary>
		/// Gets or sets a collection of temporary files to keep track of.
		/// </summary>
		public TempFileCollection TemporaryFiles { get; set; }
	}
}
