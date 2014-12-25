using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloader.ViewModels
{
	interface IShell
	{
		bool CanGoBack { get; set; }

		bool CanGoForward { get; set; }
	}
}
