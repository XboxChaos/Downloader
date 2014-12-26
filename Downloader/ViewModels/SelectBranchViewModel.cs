using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class SelectBranchViewModel : Screen
	{
		private readonly IShell _shell;
		private readonly InstallSettings _settings;
		private AvailableBranch _selectedBranch;

		[ImportingConstructor]
		public SelectBranchViewModel(IShell shell, InstallSettings settings)
		{
			_shell = shell;
			_settings = settings;
			Branches = new BindableCollection<AvailableBranch>()
			{
				new AvailableBranch() { Name = "master", Description = "(Recommended)", LatestVersion = "2014.12.34.56.78.90-master"},
				new AvailableBranch() { Name = "dev", LatestVersion = "2014.12.34.56.78.90-dev"},
			};
			SelectedBranch = Branches.FirstOrDefault(b => b.Name == settings.BranchName);
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

		protected override void OnActivate()
		{
			_shell.CanGoForward = (SelectedBranch != null);
			base.OnActivate();
		}
	}

	class AvailableBranch
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string LatestVersion { get; set; }
	}
}
