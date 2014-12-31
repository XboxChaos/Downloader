using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using XboxChaos;
using XboxChaos.Models;

namespace Downloader.ViewModels
{
	class SelectBranchViewModel : Screen
	{
		private readonly IShell _shell;
		private readonly InstallSettings _settings;
		private AvailableBranch _selectedBranch;
		private bool _showProgressBar;
		private string _statusText;

		[ImportingConstructor]
		public SelectBranchViewModel(IShell shell, InstallSettings settings)
		{
			_shell = shell;
			_settings = settings;
			Branches = new BindableCollection<AvailableBranch>();
		}

		public AvailableBranch SelectedBranch
		{
			get { return _selectedBranch; }
			set
			{
				_selectedBranch = value;
				_settings.BranchName = (_selectedBranch != null) ? _selectedBranch.Name : null;
				NotifyOfPropertyChange(() => SelectedBranch);
				_shell.CanGoForward = (_selectedBranch != null);
			}
		}

		public BindableCollection<AvailableBranch> Branches { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the loading progress bar is visible.
		/// </summary>
		public bool ShowProgressBar
		{
			get { return _showProgressBar; }
			set
			{
				_showProgressBar = value;
				NotifyOfPropertyChange(() => ShowProgressBar);
			}
		}

		/// <summary>
		/// Gets or sets the status text to display.
		/// </summary>
		public string StatusText
		{
			get { return _statusText; }
			set
			{
				_statusText = value;
				NotifyOfPropertyChange(() => StatusText);
			}
		}

		protected override async void OnInitialize()
		{
			await Load();
			base.OnInitialize();
		}

		private async Task Load()
		{
			ShowProgressBar = true;
			StatusText = "Connecting to the server...";

			_settings.ApplicationInfo = await LoadApplicationInfo();
			if (_settings.ApplicationInfo == null)
			{
				_shell.TryClose();
				return;
			}
			LoadBranches();

			ShowProgressBar = false;
			StatusText = "Please select the branch you wish to download:";
		}

		private static async Task<ApplicationResponse> LoadApplicationInfo()
		{
#if !OFFLINE
			var response = await XboxChaosApi.GetApplicationInfoAsync("Assembly"); // TODO: Retrieve the application name from configuration file
			if (response.Error == null)
				return response.Result;
			MessageBox.Show(
				"Unable to communicate with the server (" + response.Error.StatusCode + ", " + (int)response.Error.StatusCode + "):\r\n\r\n" +
				response.Error.Description, "Xbox Chaos Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
			return null;
#else
			return GetTestResponse();
#endif
		}

		private static ApplicationResponse GetTestResponse()
		{
			return new ApplicationResponse()
			{
				Name = "Assembly",
				Description = "Multi-Generation Blam Engine Research Tool",
				RepoUrl = "https://github.com/XboxChaos/Assembly",
				RepoName = "Assembly",
				ApplicationBranches = new ApplicationBranchResponse[]
				{
					new ApplicationBranchResponse()
					{
						Name = "master",
						Ref = "master",
						RepoTree = "master",
						BuildDownload = "http://mirror.internode.on.net/pub/test/10meg.test",
						UpdaterDownload = "http://mirror.internode.on.net/pub/test/1meg.test",
						FriendlyVersion = "2014.12.34.56.78.90-master",
						InternalVersion = "2.0.0.0",
						Changelog = "Test changelog",
					},
					new ApplicationBranchResponse()
					{
						Name = "dev",
						Ref = "dev",
						RepoTree = "dev",
						BuildDownload = "http://mirror.internode.on.net/pub/test/10meg.test",
						UpdaterDownload = "http://mirror.internode.on.net/pub/test/1meg.test",
						FriendlyVersion = "2014.12.34.56.78.90-dev",
						InternalVersion = "2.0.0.0",
						Changelog = "Test changelog",
					},
				},
			};
		}

		private void LoadBranches()
		{
			Branches.AddRange(_settings.ApplicationInfo.ApplicationBranches
				.Where(b => !string.IsNullOrEmpty(b.BuildDownload))
				.Select(b => new AvailableBranch()
					{
						Name = b.Name,
						Description = (b.Name == "master") ? "(Recommended)" : "",
						LatestVersion = b.FriendlyVersion
					})
				);
		}
	}

	class AvailableBranch
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string LatestVersion { get; set; }
	}
}
