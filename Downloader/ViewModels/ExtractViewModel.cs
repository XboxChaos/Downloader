﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Downloader.ViewModels
{
	class ExtractViewModel : NavigationPage
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		private readonly List<ZipArchive> _archives = new List<ZipArchive>();
		private readonly BackgroundWorker _worker = new BackgroundWorker();
		private bool _done;

		private int _totalExtracted;
		private string _currentFileName;
		private int _totalFiles;

		[ImportingConstructor]
		public ExtractViewModel(IShell shell, ApplicationSettings applicationSettings, InstallSettings installSettings)
		{
			_shell = shell;
			_applicationSettings = applicationSettings;
			_installSettings = installSettings;
		}

		protected override void OnActivate()
		{
			_shell.CanNavigate = false;
			QueueZips();
			StartExtraction();
		}

		public override void CanClose(Action<bool> callback)
		{
			if (_done)
			{
				callback(true);
				return;
			}
			var close = _shell.AskCloseQuestion("File extraction is currently in progress.\nAre you sure you want to cancel it?");
			if (close && _worker != null && _worker.IsBusy)
			{
				callback(false);
				_worker.CancelAsync();
				return;
			}
			callback(close);
		}

		protected override void OnDeactivate(bool close)
		{
			if (!close)
				_shell.CanNavigate = true;
		}

		private void QueueZips()
		{
			try
			{
				QueueZip(_installSettings.ApplicationZipPath);
				// TODO: Queue update zip if update mode is active
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to read the zip archives:\n\n" + ex.Message, "Xbox Chaos Downloader",
					MessageBoxButton.OK, MessageBoxImage.Error);
				_shell.Quit();
			}
		}

		private void QueueZip(string path)
		{
			var archive = new ZipArchive(File.OpenRead(path), ZipArchiveMode.Read, false);
			_archives.Add(archive);
			TotalFiles += archive.Entries.Count;
		}

		private void StartExtraction()
		{
			_worker.WorkerSupportsCancellation = true;
			_worker.DoWork += WorkerOnDoWork;
			_worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
			_worker.RunWorkerAsync();
		}

		private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_done = true;

			foreach (var archive in _archives)
				archive.Dispose();
			_archives.Clear();
			_installSettings.TemporaryFiles.Delete();

			if (e.Cancelled)
			{
				_shell.Quit();
				return;
			}
			if (e.Error != null)
			{
				MessageBox.Show("An error occurred while attempting to extract:\n\n" + e.Error,
					"Xbox Chaos Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
				_shell.Quit();
				return;
			}
			_shell.GoForward();
		}

		private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
		{
			foreach (var archive in _archives)
			{
				ExtractArchive(archive);
				if (_worker.CancellationPending)
					break;
			}
			e.Cancel = _worker.CancellationPending;
		}

		private void ExtractArchive(ZipArchive archive)
		{
			try
			{
				foreach (var entry in archive.Entries)
				{
					if (_worker.CancellationPending)
						return;
					CurrentFileName = entry.FullName;
					var outPath = Path.Combine(_installSettings.InstallFolder, entry.FullName);
					if (outPath.EndsWith("\\") || outPath.EndsWith("/"))
						Directory.CreateDirectory(outPath);
					else
						entry.ExtractToFile(outPath, true);
					TotalExtracted++;
				}
			}
			catch (Exception ex)
			{
				Execute.OnUIThread(() => MessageBox.Show("Unable to extract the zip archives:\n\n" + ex.Message, "Xbox Chaos Downloader",
					MessageBoxButton.OK, MessageBoxImage.Error));
				_shell.Quit();
			}
		}

		/// <summary>
		/// Gets or sets the number of files that have been extracted so far.
		/// </summary>
		public int TotalExtracted
		{
			get { return _totalExtracted; }
			set
			{
				_totalExtracted = value;
				NotifyOfPropertyChange(() => TotalExtracted);
			}
		}

		/// <summary>
		/// Gets or sets the name of the file currently being extracted.
		/// </summary>
		public string CurrentFileName
		{
			get { return _currentFileName; }
			set
			{
				_currentFileName = value;
				NotifyOfPropertyChange(() => CurrentFileName);
			}
		}

		/// <summary>
		/// Gets or sets the total number of files being extracted.
		/// </summary>
		public int TotalFiles
		{
			get { return _totalFiles; }
			set
			{
				_totalFiles = value;
				NotifyOfPropertyChange(() => TotalFiles);
			}
		}
	}
}