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
		private AvailableBranch _selectedBranch;

		[ImportingConstructor]
		public SelectBranchViewModel(IShell shell)
		{
			_shell = shell;
			Branches = new BindableCollection<AvailableBranch>()
			{
				new AvailableBranch() { Name = "master (Recommended)", LatestVersion = "2014.12.34.56.78.90-master"},
				new AvailableBranch() { Name = "dev", LatestVersion = "2014.12.34.56.78.90-dev"},
			};
			SelectedBranch = Branches[0];
		}

		public AvailableBranch SelectedBranch
		{
			get { return _selectedBranch; }
			set
			{
				_selectedBranch = value;
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

		public string LatestVersion { get; set; }
	}
}
