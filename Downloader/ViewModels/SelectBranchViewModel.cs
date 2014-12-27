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
			await LoadApplicationInfo();
			base.OnInitialize();
		}

		private async Task LoadApplicationInfo()
		{
			ShowProgressBar = true;
			StatusText = "Connecting to the server...";
			var response = await XboxChaosApi.GetApplicationInfoAsync("Assembly"); // TODO: Retrieve the application name from configuration file
			if (response.Error != null)
			{
				MessageBox.Show(
					"Unable to communicate with the server (" + response.Error.StatusCode + ", " + (int)response.Error.StatusCode + "):\r\n\r\n" +
					response.Error.Description, "Xbox Chaos Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
				_shell.TryClose();
				return;
			}
			_settings.ApplicationInfo = response.Result;
			LoadBranches();
			ShowProgressBar = false;
			StatusText = "Please select the branch you wish to download:";
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
