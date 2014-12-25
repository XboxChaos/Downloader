using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class AppViewModel : Conductor<object>
	{
		public AppViewModel()
		{
			base.DisplayName = "Xbox Chaos Downloader";
		}

		protected override void OnActivate()
		{
			ActivateItem(new SelectBranchViewModel());
		}

		public bool CanBack
		{
			get { return false; }
		}

		public void Back()
		{
			
		}

		public bool CanNext
		{
			get { return true; }
		}

		public void Next()
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
