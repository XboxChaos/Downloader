using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace Downloader.ViewModels
{
	[Export(typeof(IShell))]
	class AppViewModel : Conductor<object>, IShell
	{
		private const string SettingsFile = "app.json";

		private readonly InstallSettings _installSettings;
		private ApplicationSettings _applicationSettings;
		private NavigationViewModel _navigation;

		[ImportingConstructor]
		public AppViewModel(InstallSettings settings)
		{
			_installSettings = settings;
		}

		protected override void OnInitialize()
		{
			DisplayName = "Xbox Chaos Downloader";
			try
			{
				_applicationSettings = LoadSettings();
				_installSettings.BranchName = _applicationSettings.DefaultBranch;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to read settings from " + SettingsFile + ":\n\n" + ex.Message, DisplayName,
					MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
				return;
			}
			ActivateItem(new ConnectViewModel(this, _applicationSettings, _installSettings));
		}

		protected override void OnDeactivate(bool close)
		{
			_installSettings.TemporaryFiles.Delete();
		}

		/// <summary>
		/// Loads the settings file.
		/// </summary>
		/// <returns>The settings that were loaded.</returns>
		private static ApplicationSettings LoadSettings()
		{
			var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
			var settingsPath = Path.Combine(assemblyDir, SettingsFile);
			var json = File.ReadAllText(settingsPath);
			return JsonConvert.DeserializeObject<ApplicationSettings>(json);
		}

		public bool CanNavigate
		{
			get { return (_navigation != null) && _navigation.CanNavigate; }
			set
			{
				if (_navigation != null)
					_navigation.CanNavigate = value;
			}
		}

		public bool CanGoForward
		{
			get { return (_navigation != null) && _navigation.CanGoForward; }
			set
			{
				if (_navigation != null)
					_navigation.CanGoForward = value;
			}
		}

		public void GoForward()
		{
			if (_navigation != null)
			{
				_navigation.GoForward();
			}
			else
			{
				_navigation = new NavigationViewModel(this, _applicationSettings, _installSettings);
				ActivateItem(_navigation);
			}
		}
	}
}
