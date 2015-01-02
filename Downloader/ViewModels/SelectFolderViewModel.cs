using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;

namespace Downloader.ViewModels
{
	class SelectFolderViewModel : NavigationPage
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		[ImportingConstructor]
		public SelectFolderViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;
		}

		protected override void OnInitialize()
		{
			// By default, the install path is set to the folder that the update manager is in
			// This is fine for quick mode, but in manual mode it's probably better to put the download in a subfolder
			// So if this view is shown, then append a subdirectory named after the application onto the default path
			InstallPath = Path.Combine(InstallPath, _installSettings.ApplicationInfo.Name);
		}

		protected override void OnActivate()
		{
			CheckPath();
		}

		public string InstallPath
		{
			get { return _installSettings.InstallFolder; }
			set
			{
				_installSettings.InstallFolder = value;
				NotifyOfPropertyChange(() => InstallPath);
				CheckPath();
			}
		}

		public string ApplicationName
		{
			get { return _installSettings.ApplicationInfo.Name; }
		}

		public void Browse()
		{
			var browser = new VistaFolderBrowserDialog()
			{
				ShowNewFolderButton = true,
				SelectedPath = InstallPath
			};
			var result = browser.ShowDialog();
			if (result.HasValue && (bool) result)
				InstallPath = browser.SelectedPath;
		}

		public void CheckPath()
		{
			// Gray out the Next button if the path is invalid
			_shell.CanGoForward = PathUtil.IsDirectoryPathValid(InstallPath);
		}

		/// <summary>
		/// Called when the user advances to the next page.
		/// </summary>
		/// <returns>
		///   <c>true</c> if the user can go to the next page, or <c>false</c> to cancel.
		/// </returns>
		public override bool OnForward()
		{
			if (Directory.Exists(InstallPath))
				return true;

			// Directory doesn't exist - ask the user to create it, and fail if they choose not to
			var result = MessageBox.Show("The selected folder does not exist. Do you want to create it?",
				"Xbox Chaos Downloader", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
			if (result == MessageBoxResult.No)
				return false;
			try
			{
				// Attempt to create the directory
				Directory.CreateDirectory(InstallPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to create the folder:\n\n" + ex.Message, "Xbox Chaos Downloader", MessageBoxButton.OK,
					MessageBoxImage.Error);
				return false;
			}
			return true;
		}
	}
}
