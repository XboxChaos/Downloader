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
		private readonly ForceCloseStrategy<object> _forceClose;

		private bool _initialized = false;      // True if initialized
		private bool _quitting = false;         // True if Quit() was called
		private bool _waitingForUser = false;   // True if the user is being asked for input
		private bool _waitingToAdvance = false; // True if we should move to the next page when the user is no longer being asked for input
		private ErrorViewModel _error;          // Non-null if an error occurred

		[ImportingConstructor]
		public AppViewModel(InstallSettings settings)
		{
			_installSettings = settings;
			_forceClose = new ForceCloseStrategy<object>(CloseStrategy);
			CloseStrategy = _forceClose;
		}

		protected override void OnInitialize()
		{
			DisplayName = "Downloader";
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
			_initialized = true;
		}

		protected override void OnDeactivate(bool close)
		{
			try
			{
				_installSettings.TemporaryFiles.Delete();
			}
			catch (Exception)
			{
			}
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
			if (_error != null)
				return;
			if (_waitingForUser)
			{
				_waitingToAdvance = true;
				return;
			}
			if (_navigation != null)
			{
				_navigation.GoForward();
			}
			else
			{
				_navigation = new NavigationViewModel(this, _applicationSettings, _installSettings);
				ActivateItem(_navigation);
				if (_error != null)
					ActivateItem(_error); // In case an error occurred during ActivateItem
			}
		}

		public bool AskCloseQuestion(string question)
		{
			if (_quitting)
				return true;
			if (_waitingForUser)
				return false;

			_waitingForUser = true;
			var result = MessageBox.Show(question, DisplayName, MessageBoxButton.YesNo, MessageBoxImage.Question,
				MessageBoxResult.No);
			_waitingForUser = false;

			if (result == MessageBoxResult.Yes)
				return true;

			if (_waitingToAdvance)
				GoForward();
			return false;
		}

		public void Quit()
		{
			_quitting = true;
			TryClose();
		}

		public void ShowError(string caption, string details)
		{
			// If the application isn't initialized yet, fall back on a standard message box
			if (!_initialized)
			{
				MessageBox.Show("Error: " + caption + Environment.NewLine + Environment.NewLine + details, DisplayName,
					MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
				return;
			}

			_forceClose.Enabled = true; // Force all pages to close
			_error = new ErrorViewModel(this, caption, details);
			ActivateItem(_error);
		}
	}

	/// <summary>
	/// ICloseStrategy which allows closing to be forced.
	/// </summary>
	/// <typeparam name="T">The type of object to manage.</typeparam>
	class ForceCloseStrategy<T> : ICloseStrategy<T>
	{
		private readonly ICloseStrategy<T> _baseStrategy;

		public ForceCloseStrategy(ICloseStrategy<T> baseStrategy)
		{
			_baseStrategy = baseStrategy;
		}

		/// <summary>
		/// Gets or sets a value indicating whether closing should be forced.
		/// </summary>
		public bool Enabled { get; set; }
 
		public void Execute(IEnumerable<T> toClose, Action<bool, IEnumerable<T>> callback)
		{
			if (Enabled)
				callback(true, Enumerable.Empty<T>());
			else
				_baseStrategy.Execute(toClose, callback);
		}
	}
}
