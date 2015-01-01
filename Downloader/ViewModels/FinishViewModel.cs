using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class FinishViewModel : Screen
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		private bool _runOnClose = true;

		[ImportingConstructor]
		public FinishViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;
		}

		public string ApplicationName
		{
			get { return _installSettings.ApplicationInfo.Name; }
		}

		public string ApplicationVersion
		{
			get
			{
				return
					_installSettings.ApplicationInfo.ApplicationBranches.First(b => b.Name == _installSettings.BranchName)
						.FriendlyVersion;
			}
		}

		public string ExtractionPath
		{
			get { return _installSettings.InstallFolder; }
		}

		public bool RunOnClose
		{
			get { return _runOnClose; }
			set
			{
				_runOnClose = value;
				NotifyOfPropertyChange(() => RunOnClose);
			}
		}
	}
}
