using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	interface IShell
	{
		bool CanNavigate { get; set; }

		bool CanGoForward { get; set; }

		void GoForward();

		void Quit();

		bool AskCloseQuestion(string question);
	}
}
