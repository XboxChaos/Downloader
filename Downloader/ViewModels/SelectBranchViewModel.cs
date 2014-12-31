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
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;
		private AvailableBranch _selectedBranch;

		[ImportingConstructor]
		public SelectBranchViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;
			Branches = new BindableCollection<AvailableBranch>();
		}

		public AvailableBranch SelectedBranch
		{
			get { return _selectedBranch; }
			set
			{
				_selectedBranch = value;
				_installSettings.BranchName = (_selectedBranch != null) ? _selectedBranch.Name : null;
				_shell.CanGoForward = (_selectedBranch != null);
				NotifyOfPropertyChange(() => SelectedBranch);
			}
		}

		public BindableCollection<AvailableBranch> Branches { get; set; }

		protected override void OnInitialize()
		{
			LoadBranches();
			SelectDefaultBranch();
		}

		private void LoadBranches()
		{
			Branches.AddRange(_installSettings.ApplicationInfo.ApplicationBranches
				.Where(b => !string.IsNullOrEmpty(b.BuildDownload))
				.Select(b => new AvailableBranch()
					{
						Name = b.Name,
						Description = (b.Name == "master") ? "(Recommended)" : "",
						LatestVersion = b.FriendlyVersion
					})
				);
		}

		private void SelectDefaultBranch()
		{
			SelectedBranch = Branches.FirstOrDefault(b => b.Name == _installSettings.BranchName);
		}
	}

	class AvailableBranch
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string LatestVersion { get; set; }
	}
}
