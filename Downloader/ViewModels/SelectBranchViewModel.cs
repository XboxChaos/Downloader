using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class SelectBranchViewModel : Screen
	{
		public SelectBranchViewModel()
		{
			Branches = new BindableCollection<AvailableBranch>()
			{
				new AvailableBranch() { Name = "master (Recommended)", LatestVersion = "2014.12.34.56.78.90-master"},
				new AvailableBranch() { Name = "dev", LatestVersion = "2014.12.34.56.78.90-dev"},
			};
			SelectedBranch = Branches[0];
		}

		public AvailableBranch SelectedBranch { get; set; }

		public BindableCollection<AvailableBranch> Branches { get; set; } 
	}

	class AvailableBranch
	{
		public string Name { get; set; }

		public string LatestVersion { get; set; }
	}
}
