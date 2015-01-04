using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class ErrorViewModel : Screen
	{
		private readonly IShell _shell;

		[ImportingConstructor]
		public ErrorViewModel(IShell shell, string caption, string details)
		{
			_shell = shell;
			CaptionText = "Error: " + caption;
			DetailText = details;
		}

		public string CaptionText { get; private set; }

		public string DetailText { get; private set; }

		public void Quit()
		{
			_shell.Quit();
		}

		public void Copy()
		{
			Clipboard.SetText(CaptionText + Environment.NewLine + Environment.NewLine + DetailText);
		}

		public void ReportIssue()
		{
			Process.Start("https://github.com/xboxchaos/downloader/issues");
		}
	}
}
