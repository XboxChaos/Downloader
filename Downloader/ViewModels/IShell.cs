using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.ViewModels
{
	interface IShell
	{
		bool CanGoForward { get; set; }

		void GoForward();
	}
}
