using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Downloader
{
	/// <summary>
	/// Holds information about the application to be downloaded.
	/// </summary>
	[JsonObject]
	class ApplicationSettings
	{
		/// <summary>
		/// Gets or sets the (internal) name of the application to download.
		/// </summary>
		[JsonProperty("application_name")]
		public string ApplicationName { get; set; }

		/// <summary>
		/// Gets or sets the branch to select by default.
		/// </summary>
		[JsonProperty("default_branch")]
		public string DefaultBranch { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether quick mode should be activated.
		/// </summary>
		[JsonProperty("quick")]
		public bool QuickMode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the download should be treated as an update.
		/// </summary>
		public bool Update { get; set; }
	}
}
