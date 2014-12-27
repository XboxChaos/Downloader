using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	interface IShell : IClose
	{
		bool CanNavigate { get; set; }

		bool CanGoForward { get; set; }

		void GoForward();
	}
}
