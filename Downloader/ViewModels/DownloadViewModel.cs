using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class DownloadViewModel : Screen
	{
		private const double UpdateInterval = 0.5;

		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		private readonly WebClient _client = new WebClient(); // WebClient to use
		private DateTime _lastTime;                           // The last time a progress update occurred
		private long _lastSize;                               // The last file size recorded
		private bool _done;                                   // True if downloading is complete
		private bool _waitingForUser;                         // True if a modal dialog is waiting for user input
		private DownloadRequest[] _downloadQueue;             // The files that need to be downloaded
		private int _currentDownload = 0;                     // Index of the current file being downloaded

		private string _applicationName;
		private int _percentComplete;
		private string _downloadedSize;
		private string _totalSize;
		private string _downloadSpeed;
		private bool _displayProgress;

		[ImportingConstructor]
		public DownloadViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;

			_client.DownloadProgressChanged += OnDownloadProgressChanged;
			_client.DownloadFileCompleted += OnDownloadFileCompleted;
		}

		protected override void OnActivate()
		{
			_shell.CanNavigate = false;
			QueueDownloads();
			BeginDownload();
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			/*if (PercentComplete != 100)
				return;*/
			foreach (var download in _downloadQueue)
			{
				if (File.Exists(download.ResultPath))
					File.Delete(download.ResultPath);
			}
		}

		public override void CanClose(Action<bool> callback)
		{
			if (_done)
			{
				callback(true);
				return;
			}

			// Ask the user if they want to close first
			_waitingForUser = true;
			var result = MessageBox.Show("A file download is currently in progress.\r\nAre you sure you want to cancel it?",
				"Xbox Chaos Downloader", MessageBoxButton.YesNo, MessageBoxImage.Question);
			_waitingForUser = false;
			if (result == MessageBoxResult.Yes)
			{
				// User wants to close - close if the download has already completed, or cancel the download otherwise
				callback(_done);
				_client.CancelAsync();
				return;
			}

			// User doesn't want to close - signal that we're finished if the download has already completed
			callback(false);
			if (_done)
				Finished();
		}

		private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				// A cancelled download indicates that the user wants to quit
				_done = true;
				_shell.TryClose();
				return;
			}
			if (e.Error != null)
			{
				MessageBox.Show("An error occurred while attempting to download " + ApplicationName + ":\r\n\r\n" + e.Error,
					"Xbox Chaos Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
				_done = true;
				_shell.TryClose();
				return;
			}

			// Download succeeded
			DownloadedSize = TotalSize;
			PercentComplete = 100;
			_currentDownload++;
			NotifyOfPropertyChange(() => CurrentFileNumber);
			if (_currentDownload == _downloadQueue.Length)
			{
				_done = true;
				if (!_waitingForUser)
					Finished(); // Signal that we're finished if we're not waiting for user input
				return;
			}
			BeginDownload();
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			// Only update the UI in fixed intervals
			var currentTime = DateTime.Now;
			var elapsed = currentTime.Subtract(_lastTime).TotalSeconds;
			if (_lastSize > 0 && elapsed < UpdateInterval)
				return;

			DownloadedSize = SizeUtil.FormatSizeForDisplay(e.BytesReceived);
			TotalSize = SizeUtil.FormatSizeForDisplay(e.TotalBytesToReceive);
			PercentComplete = (int)(100 * e.BytesReceived / e.TotalBytesToReceive);

			if (elapsed >= UpdateInterval)
			{
				DownloadSpeed = SizeUtil.FormatRateForDisplay(e.BytesReceived - _lastSize, elapsed);
				_lastSize = e.BytesReceived;
				_lastTime = currentTime;
			}

			DisplayProgress = true;
		}

		/// <summary>
		/// Builds the download queue based off of information for the current branch.
		/// </summary>
		private void QueueDownloads()
		{
			var branch = _installSettings.ApplicationInfo.ApplicationBranches.FirstOrDefault(b => b.Name == _installSettings.BranchName);
			if (branch == null)
			{
				MessageBox.Show("Unable to find the branch to download!", "Xbox Chaos Downloader", MessageBoxButton.OK,
					MessageBoxImage.Error);
				TryClose();
				return;
			}
			if (string.IsNullOrEmpty(branch.BuildDownload) || string.IsNullOrEmpty(branch.UpdaterDownload))
			{
				MessageBox.Show("The branch has invalid download links!", "Xbox Chaos Downloader", MessageBoxButton.OK,
					MessageBoxImage.Error);
				TryClose();
				return;
			}

			// Create two queued downloads for the branch - one for the actual program, and one for the updater
			_downloadQueue = new[]
			{
				// Download for the actual program
				new DownloadRequest()
				{
					DisplayName = _installSettings.ApplicationInfo.Name,
					Url = branch.BuildDownload,
					ResultPath = Path.GetTempFileName(),
				},

				// Download for the updater
				new DownloadRequest()
				{
					DisplayName = "updater",
					Url = branch.UpdaterDownload,
					ResultPath = Path.GetTempFileName(),
				},
			};
			NotifyOfPropertyChange(() => TotalFiles);
		}

		/// <summary>
		/// Begins downloading the current file.
		/// </summary>
		private void BeginDownload()
		{
			var download = _downloadQueue[_currentDownload];
			ApplicationName = download.DisplayName;
			DownloadSpeed = "0 B/s";
			_lastSize = 0;
			_lastTime = DateTime.Now;
			_client.DownloadFileAsync(new Uri(download.Url), download.ResultPath);
		}

		/// <summary>
		/// Moves to the next step.
		/// </summary>
		private void Finished()
		{
			_shell.CanNavigate = true;
			_shell.GoForward();
		}

		/// <summary>
		/// Gets or sets the name of the application to display.
		/// </summary>
		public string ApplicationName
		{
			get { return _applicationName; }
			set
			{
				_applicationName = value;
				NotifyOfPropertyChange(() => ApplicationName);
			}
		}

		/// <summary>
		/// Gets or sets the percent complete out of 100.
		/// </summary>
		public int PercentComplete
		{
			get { return _percentComplete; }
			set
			{
				_percentComplete = value;
				NotifyOfPropertyChange(() => PercentComplete);
			}
		}

		/// <summary>
		/// Gets or sets a string displaying how much has been downloaded.
		/// </summary>
		public string DownloadedSize
		{
			get { return _downloadedSize; }
			set
			{
				_downloadedSize = value;
				NotifyOfPropertyChange(() => DownloadedSize);
			}
		}

		/// <summary>
		/// Gets or sets a string displaying the total size that needs to be downloaded.
		/// </summary>
		public string TotalSize
		{
			get { return _totalSize; }
			set
			{
				_totalSize = value;
				NotifyOfPropertyChange(() => TotalSize);
			}
		}

		/// <summary>
		/// Gets or sets a string displaying the speed of the download.
		/// </summary>
		public string DownloadSpeed
		{
			get { return _downloadSpeed; }
			set
			{
				_downloadSpeed = value;
				NotifyOfPropertyChange(() => DownloadSpeed);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to display download progress.
		/// </summary>
		public bool DisplayProgress
		{
			get { return _displayProgress; }
			set
			{
				_displayProgress = value;
				NotifyOfPropertyChange(() => DisplayProgress);
			}
		}

		/// <summary>
		/// Gets the number of the file currently being downloaded, starting from 1.
		/// </summary>
		public int CurrentFileNumber
		{
			get { return _currentDownload + 1; }
		}

		/// <summary>
		/// Gets or the total number of files to be downloaded.
		/// </summary>
		public int TotalFiles
		{
			get { return _downloadQueue.Length; }
		}
	}

	/// <summary>
	/// A file that needs to be downloaded.
	/// </summary>
	class DownloadRequest
	{
		/// <summary>
		/// Gets or sets the name to display for the download.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the URL of the file to download.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Gets or sets the path of the resulting file.
		/// </summary>
		public string ResultPath { get; set; }
	}
}
