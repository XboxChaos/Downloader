using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	[Export(typeof(IShell))]
	class AppViewModel : Conductor<object>.Collection.OneActive, IShell
	{
		private bool _canNext;
		private bool _canBack;

		public AppViewModel()
		{
			base.DisplayName = "Xbox Chaos Downloader";
		}

		protected override void OnActivate()
		{
			ActivateItem(new SelectBranchViewModel(this));
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
