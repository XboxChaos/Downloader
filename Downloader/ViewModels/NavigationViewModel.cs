using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;
using XboxChaos;

namespace Downloader.ViewModels
{
	class NavigationViewModel : Conductor<object>.Collection.OneActive
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;
		private readonly Screen[] _screens;
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
			_screens = new Screen[]
			{
				new SelectBranchViewModel(shell, applicationSettings, installSettings),
 				new SelectFolderViewModel(shell, applicationSettings, installSettings), 
				new DownloadViewModel(shell, applicationSettings, installSettings), 
				new ExtractViewModel(shell, applicationSettings, installSettings), 
				new FinishViewModel(shell, applicationSettings, installSettings), 
			};
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
			_currentScreen++;
			CanGoBack = (_currentScreen > 0 && !(_screens[_currentScreen - 1] is ExtractViewModel)); // TODO: fix hax
			if (_currentScreen >= _screens.Length)
			{
				// Close after the last screen
				_shell.TryClose();
				return;
			}
			ActivateItem(_screens[_currentScreen]);
			NotifyOfPropertyChange(() => ForwardText);
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
	}
}
