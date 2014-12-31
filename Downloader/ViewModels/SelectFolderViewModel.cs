using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;

namespace Downloader.ViewModels
{
	class SelectFolderViewModel : Screen
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

		public string InstallPath
		{
			get { return _installSettings.InstallFolder; }
			set
			{
				_installSettings.InstallFolder = value;
				NotifyOfPropertyChange(() => InstallPath);
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
	}
}
