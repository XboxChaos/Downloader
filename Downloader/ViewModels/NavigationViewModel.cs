using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class NavigationViewModel : Conductor<NavigationPage>.Collection.OneActive
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;
		private NavigationPage[] _screens;
		private int _currentScreen;

		private bool _canNext;
		private bool _canBack;
		private bool _canNavigate = true;

		[ImportingConstructor]
		public NavigationViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;

			if (applicationSettings.QuickMode)
				InitQuickMode();
			else
				InitManualMode();

			base.ActivateItem(_screens[0]);
		}

		public string ApplicationName
		{
			get { return _installSettings.ApplicationInfo.Name; }
		}

		public string ApplicationDescription
		{
			get { return _installSettings.ApplicationInfo.Description; }
		}

		public bool CanNavigate
		{
			get { return _canNavigate; }
			set
			{
				_canNavigate = value;
				NotifyOfPropertyChange(() => CanNavigate);
			}
		}

		public bool CanGoBack
		{
			get { return _canBack; }
			set
			{
				_canBack = value;
				NotifyOfPropertyChange(() => CanGoBack);
			}
		}

		public void GoBack()
		{
			if (_currentScreen <= 0)
				return;
			_currentScreen--;
			CanGoBack = (_currentScreen > 0);
			ActivateItem(_screens[_currentScreen]);
			NotifyOfPropertyChange(() => ForwardText);
		}

		public bool CanGoForward
		{
			get { return _canNext; }
			set
			{
				_canNext = value;
				NotifyOfPropertyChange(() => CanGoForward);
			}
		}

		public void GoForward()
		{
			// Call the current screen's OnForward() method and cancel if it returns false
			var currentPage = ActiveItem;
			if (currentPage != null && !currentPage.OnForward())
				return;

			// If this is the last screen, then quit
			if (_currentScreen >= _screens.Length - 1)
			{
				Finish();
				return;
			}

			// Advance to the next screen and update the UI
			_currentScreen++;
			CanGoBack = (_currentScreen > 0 && !(_screens[_currentScreen - 1] is ExtractViewModel)); // TODO: fix hax
			ActivateItem(_screens[_currentScreen]);
			NotifyOfPropertyChange(() => ForwardText);
		}

		public void Finish()
		{
			if (_installSettings.RunOnFinish)
				RunApplication();
			_shell.Quit();
		}

		public void RunApplication()
		{
			// Assume there's an executable in the install directory named after the application
			var exePath = Path.Combine(_installSettings.InstallFolder, _installSettings.ApplicationInfo.Name + ".exe");
			try
			{
				var startInfo = new ProcessStartInfo(exePath)
				{
					WorkingDirectory = _installSettings.InstallFolder,
				};
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to start the application:\n\n" + ex.Message, "Xbox Chaos Downloader", MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		public string ForwardText
		{
			get { return (_currentScreen < _screens.Length - 1) ? "Next" : "Finish"; }
		}

		public void XboxChaos()
		{
			Process.Start("http://www.xboxchaos.com/");
		}

		public void GitHub()
		{
			Process.Start("https://www.github.com/XboxChaos");
		}

		private void InitManualMode()
		{
			_screens = new NavigationPage[]
			{
				new SelectBranchViewModel(_shell, _applicationSettings, _installSettings),
 				new SelectFolderViewModel(_shell, _applicationSettings, _installSettings), 
				new DownloadViewModel(_shell, _applicationSettings, _installSettings), 
				new ExtractViewModel(_shell, _applicationSettings, _installSettings), 
				new FinishViewModel(_shell, _applicationSettings, _installSettings), 
			};
		}

		private void InitQuickMode()
		{
			_screens = new NavigationPage[]
			{
				new DownloadViewModel(_shell, _applicationSettings, _installSettings), 
				new ExtractViewModel(_shell, _applicationSettings, _installSettings), 
			};
		}
	}
}
