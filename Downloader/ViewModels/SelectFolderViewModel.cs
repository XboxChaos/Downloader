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

namespace Downloader.ViewModels
{
	class SelectFolderViewModel : Screen
	{
		private readonly IShell _shell;
		private string _installPath;

		[ImportingConstructor]
		public SelectFolderViewModel(IShell shell)
		{
			_shell = shell;
			InstallPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		public string InstallPath
		{
			get { return _installPath; }
			set
			{
				_installPath = value;
				NotifyOfPropertyChange(() => InstallPath);
			}
		}

		public void Browse()
		{
			// TODO: Implement
		}
	}
}
