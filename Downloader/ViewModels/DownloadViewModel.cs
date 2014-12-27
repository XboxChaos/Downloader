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
		private readonly InstallSettings _settings;

		private WebClient _client;                  // WebClient to use
		private string _outPath;                    // Path to download the file to
		private DateTime _lastTime;                 // The last time a progress update occurred
		private long _lastSize;                     // The last file size recorded
		private volatile bool _done;                // True if downloading has stopped
		private volatile bool _waitingForUser;      // True if a modal dialog is waiting for user input
		private Action<bool> _pendingCloseCallback; // If the user wants to close, this is the callback to execute

		private string _applicationName;
		private int _percentComplete;
		private string _downloadedSize;
		private string _totalSize;
		private string _downloadSpeed;
		private bool _displayProgress;

		[ImportingConstructor]
		public DownloadViewModel(IShell shell, InstallSettings settings)
		{
			_shell = shell;
			_settings = settings;
		}

		protected override void OnActivate()
		{
			_shell.CanNavigate = false;
			ApplicationName = "Assembly";
			DownloadSpeed = "0 B/s";
			_outPath = Path.GetTempFileName();
			_lastSize = 0;
			_lastTime = DateTime.Now;

			_client = new WebClient();
			_client.DownloadProgressChanged += OnDownloadProgressChanged;
			_client.DownloadFileCompleted += OnDownloadFileCompleted;
			_client.DownloadFileAsync(new Uri("http://mirror.internode.on.net/pub/test/100meg.test"), _outPath); // 100 MB test file for now
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			/*if (PercentComplete != 100)
				return;*/
			if (File.Exists(_outPath))
				File.Delete(_outPath);
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
			if (_done || result == MessageBoxResult.No)
			{
				callback(_done);
				return;
			}

			// Store the callback and wait for the WebClient to cancel
			_pendingCloseCallback = callback;
			_client.CancelAsync();
		}

		private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			_done = true;
			if (e.Error == null && !e.Cancelled)
			{
				// Download succeeded
				DownloadedSize = TotalSize;
				PercentComplete = 100;

				// If we're not waiting for the user to do anything, then try to close
				if (!_waitingForUser)
					_shell.TryClose();
				return;
			}
			if (!e.Cancelled)
			{
				MessageBox.Show("An error occurred while attempting to download " + ApplicationName + ":\r\n\r\n" + e.Error,
					"Xbox Chaos Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
				_shell.TryClose();
			}

			// If the user requested to quit, then run the pending callback
			if (_pendingCloseCallback != null)
				_pendingCloseCallback(true);
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
	}
}
