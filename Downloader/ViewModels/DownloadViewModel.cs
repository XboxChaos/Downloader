﻿using System;
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
	class DownloadViewModel : NavigationPage
	{
		private const double UpdateInterval = 0.5;

		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		private readonly WebClient _client = new WebClient(); // WebClient to use
		private DateTime _lastTime;                           // The last time a progress update occurred
		private long _lastSize;                               // The last file size recorded
		private List<DownloadRequest> _downloadQueue;         // The files that need to be downloaded
		private int _currentDownload;                         // Index of the current file being downloaded
		private bool _done;                                   // True if done

		private string _applicationName;
		private int _percentComplete;
		private string _downloadedSize;
		private string _totalSize;
		private string _downloadSpeed;
		private bool _displayProgress;
		private string _timeRemaining;

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
			if (QueueDownloads())
				BeginDownload();
		}

		protected override void OnDeactivate(bool close)
		{
			if (!close)
				_shell.CanNavigate = true;
		}

		public override void CanClose(Action<bool> callback)
		{
			if (_done)
			{
				callback(true);
				return;
			}
			var close = _shell.AskCloseQuestion("A file download is currently in progress.\nAre you sure you want to cancel it?");
			if (close && _client.IsBusy)
			{
				callback(false);
				_client.CancelAsync();
				return;
			}
			callback(close);
		}

		private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			_done = true;
			if (e.Cancelled)
			{
				// A cancelled download indicates that the user wants to quit
				_shell.Quit();
				return;
			}
			if (e.Error != null)
			{
				_shell.ShowError("Unable to download " + ApplicationName + "!", e.Error.ToString());
				return;
			}

			// Download succeeded - show 100% completion in the UI
			DownloadedSize = TotalSize;
			PercentComplete = 100;

			// Check if this was the last download
			if (_currentDownload == _downloadQueue.Count - 1)
			{
				Finished();
				return;
			}

			// Move to the next download
			_currentDownload++;
			NotifyOfPropertyChange(() => CurrentFileNumber);
			BeginDownload();
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			// Only update the UI in fixed intervals
			var currentTime = DateTime.Now;
			var elapsed = currentTime.Subtract(_lastTime).TotalSeconds;
			if (_lastSize > 0 && elapsed < UpdateInterval)
				return;

			DownloadedSize = SizeDisplay.Format(e.BytesReceived);
			TotalSize = SizeDisplay.Format(e.TotalBytesToReceive);
			PercentComplete = (int)(100 * e.BytesReceived / e.TotalBytesToReceive);

			if (elapsed >= UpdateInterval)
			{
				// Calculate download speed
				var bytesTransferred = e.BytesReceived - _lastSize;
				DownloadSpeed = SizeDisplay.FormateRate(bytesTransferred, elapsed);

				// Calculate time remaining
				var bytesPerSecond = (long)(bytesTransferred / elapsed);
				var bytesRemaining = e.TotalBytesToReceive - e.BytesReceived;
				var secondsRemaining = bytesRemaining / bytesPerSecond;
				var remaining = new TimeSpan(0, 0, (int)secondsRemaining);
				TimeRemaining = remaining.ToString();

				_lastSize = e.BytesReceived;
				_lastTime = currentTime;
			}

			DisplayProgress = true;
		}

		/// <summary>
		/// Builds the download queue based off of information for the current branch.
		/// </summary>
		/// <returns><c>true</c> if successful.</returns>
		private bool QueueDownloads()
		{
			var branch = _installSettings.ApplicationInfo.ApplicationBranches.FirstOrDefault(b => b.Name == _installSettings.BranchName);
			if (branch == null)
			{
				_done = true;
				_shell.ShowError("Unable to find the branch to download!", "branch is null");
				return false;
			}
			if (string.IsNullOrEmpty(branch.BuildDownload) || string.IsNullOrEmpty(branch.UpdaterDownload))
			{
				_done = true;
				_shell.ShowError("The branch has invalid download links!", "BuildDownload or UpdaterDownload is empty");
				return false;
			}

			// Queue a download for the actual program
			_installSettings.ApplicationZipPath = Path.GetTempFileName();
			_installSettings.TemporaryFiles.AddFile(_installSettings.ApplicationZipPath, _applicationSettings.Update); // Keep the application zip in update mode
			_downloadQueue = new List<DownloadRequest>()
			{
				new DownloadRequest()
				{
					DisplayName = _installSettings.ApplicationInfo.Name,
					Url = branch.BuildDownload,
					ResultPath = _installSettings.ApplicationZipPath,
				}
			};
			if (_applicationSettings.Update)
			{
				// Queue a download for the updater if update mode is active
				_installSettings.UpdateZipPath = Path.GetTempFileName();
				_installSettings.TemporaryFiles.AddFile(_installSettings.UpdateZipPath, false);
				_downloadQueue.Add(new DownloadRequest()
				{
					DisplayName = "updater",
					Url = branch.UpdaterDownload,
					ResultPath = _installSettings.UpdateZipPath,
				});
			};
			NotifyOfPropertyChange(() => TotalFiles);
			return true;
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
		/// Gets or sets a string displaying the amount of time remaining.
		/// </summary>
		public string TimeRemaining
		{
			get { return _timeRemaining; }
			set
			{
				_timeRemaining = value;
				NotifyOfPropertyChange(() => TimeRemaining);
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
			get { return _downloadQueue.Count; }
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
