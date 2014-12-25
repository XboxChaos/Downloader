using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Downloader.ViewModels;

namespace Downloader
{
	class AppBootstrapper : BootstrapperBase
	{
		public AppBootstrapper()
		{
			Initialize();
		}

		protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
		{
			DisplayRootViewFor<AppViewModel>();
			Application.MainWindow.ResizeMode = ResizeMode.CanMinimize;
		}
	}
}
