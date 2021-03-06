﻿using System;
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
				return;
			_shell.GoForward();
		}

		private async Task<ApplicationResponse> LoadApplicationInfo()
		{
#if !OFFLINE
			var response = await XboxChaosApi.GetApplicationInfoAsync(_applicationSettings.ApplicationName);
			if (response.Error == null)
				return response.Result;
			_shell.ShowError(
				"Unable to communicate with the server! (" + response.Error.StatusCode + ", " + (int)response.Error.StatusCode + ")",
				response.Error.Description);
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
				ApplicationBranches = new List<ApplicationBranchResponse>
				{
					new ApplicationBranchResponse()
					{
						Name = "master",
						Ref = "master",
						RepoTree = "master",
						BuildDownload = "http://xboxchaos.com/assembly/updatetest/assembly.zip",
						UpdaterDownload = "http://xboxchaos.com/assembly/updatetest/update.zip",
						Version = ApplicationVersionPair.TryParse("2014.01.02.03.04.05-master", "2.0.0.0"),
					},
					new ApplicationBranchResponse()
					{
						Name = "dev",
						Ref = "dev",
						RepoTree = "dev",
						BuildDownload = "http://xboxchaos.com/assembly/updatetest/assembly.zip",
						UpdaterDownload = "http://xboxchaos.com/assembly/updatetest/update.zip",
						Version = ApplicationVersionPair.TryParse("2014.01.02.03.04.05-dev", "2.0.0.0"),
					},
				},
			};
		}
	}
}
