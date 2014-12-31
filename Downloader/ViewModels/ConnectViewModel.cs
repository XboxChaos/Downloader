using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using XboxChaos;
using XboxChaos.Models;

namespace Downloader.ViewModels
{
	class ConnectViewModel : Screen
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		[ImportingConstructor]
		public ConnectViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;
		}

		protected override async void OnInitialize()
		{
			_installSettings.ApplicationInfo = await LoadApplicationInfo();
			if (_installSettings.ApplicationInfo == null)
			{
				_shell.TryClose();
				return;
			}
			_shell.GoForward();
		}

		private async Task<ApplicationResponse> LoadApplicationInfo()
		{
#if !OFFLINE
			var response = await XboxChaosApi.GetApplicationInfoAsync(_applicationSettings.ApplicationName);
			if (response.Error == null)
				return response.Result;
			MessageBox.Show(
				"Unable to communicate with the server (" + response.Error.StatusCode + ", " + (int)response.Error.StatusCode + "):\n\n" +
				response.Error.Description, "Xbox Chaos Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
			return null;
#else
			await Task.Delay(1000);
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
	}
}
