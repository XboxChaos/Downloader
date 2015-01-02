using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class FinishViewModel : NavigationPage
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

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

		public bool CanRunOnClose
		{
			get { return !_applicationSettings.Update; }
		}

		public bool RunOnClose
		{
			get { return _installSettings.RunOnFinish || _applicationSettings.Update; }
			set
			{
				_installSettings.RunOnFinish = value;
				NotifyOfPropertyChange(() => RunOnClose);
			}
		}
	}
}
