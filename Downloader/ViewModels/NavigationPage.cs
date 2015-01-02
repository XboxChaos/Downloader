using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	/// <summary>
	/// Base class for a page which can be navigated to and from.
	/// </summary>
	class NavigationPage : Screen
	{
		/// <summary>
		/// Called when the user advances to the next page.
		/// </summary>
		/// <returns><c>true</c> if the user can go to the next page, or <c>false</c> to cancel.</returns>
		public virtual bool OnForward()
		{
			return true;
		}
	}
}
