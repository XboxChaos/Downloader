using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using XboxChaos;

namespace Downloader.ViewModels
{
	[Export(typeof(IShell))]
	class AppViewModel : Conductor<object>.Collection.OneActive, IShell
	{
		private readonly Screen[] _screens;
		private int _currentScreen;

		private bool _canNext;
		private bool _canBack;
		private bool _canNavigate = true;

		[ImportingConstructor]
		public AppViewModel(InstallSettings settings)
		{
			base.DisplayName = "Xbox Chaos Downloader";
			_screens = new Screen[]
			{
				new SelectBranchViewModel(this, settings),
 				new SelectFolderViewModel(this, settings), 
				new DownloadViewModel(this, settings), 
			};
			base.ActivateItem(_screens[0]);
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
			CanGoBack = (_currentScreen > 0);
			if (_currentScreen < _screens.Length)
				ActivateItem(_screens[_currentScreen]);
			else
				TryClose();
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
