using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Downloader.ViewModels
{
	class UpdateViewModel : NavigationPage
	{
		private const string UpdaterName = "update.exe"; // Name of the updater program to run

		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		private volatile bool _done;

		[ImportingConstructor]
		public UpdateViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;
		}

		public override void CanClose(Action<bool> callback)
		{
			// Don't allow closing at all until the updater finishes
			callback(_done);
		}

		protected override async void OnActivate()
		{
			_shell.CanNavigate = false;
			await Task.Run(() => RunUpdater());
			_shell.CanNavigate = true;
			_shell.GoForward();
		}

		public string NewVersion
		{
			get
			{
				return
					_installSettings.ApplicationInfo.ApplicationBranches.First(b => b.Name == _installSettings.BranchName)
						.FriendlyVersion;
			}
		}

		public void RunUpdater()
		{
			var exePath = Path.Combine(Path.GetTempPath(), UpdaterName); // The updater is extracted to the temp directory
			try
			{
				// Run the updater silently and wait for it to finish
				var startInfo = new ProcessStartInfo(exePath)
				{
					Arguments = string.Format("\"{0}\" \"{1}\" {2}", _installSettings.ApplicationZipPath, _installSettings.GetApplicationExePath(), -1), // <update zip> <exe> <parent pid>
					WorkingDirectory = _installSettings.InstallFolder,
					CreateNoWindow = true,
					UseShellExecute = false
				};
				var process = Process.Start(startInfo);
				if (process != null)
					process.WaitForExit();
				_done = true;
			}
			catch (Exception ex)
			{
				_done = true;
				MessageBox.Show("Unable to start the updater:\n\n" + ex.Message, "Xbox Chaos Downloader", MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}
	}
}
