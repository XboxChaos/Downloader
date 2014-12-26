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
		private readonly InstallSettings _settings;

		[ImportingConstructor]
		public SelectFolderViewModel(IShell shell, InstallSettings settings)
		{
			_shell = shell;
			_settings = settings;
		}

		public string InstallPath
		{
			get { return _settings.InstallFolder; }
			set
			{
				_settings.InstallFolder = value;
				NotifyOfPropertyChange(() => InstallPath);
			}
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
