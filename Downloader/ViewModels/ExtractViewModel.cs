using System;
using System.Collections.Generic;
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
	class ExtractViewModel : Screen
	{
		private readonly IShell _shell;
		private readonly ApplicationSettings _applicationSettings;
		private readonly InstallSettings _installSettings;

		private readonly List<ZipArchive> _archives = new List<ZipArchive>();

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

		protected override async void OnActivate()
		{
			_shell.CanNavigate = false;

			QueueZips();
			await ExtractArchives();

			_shell.CanNavigate = true;
			_shell.GoForward();
		}

		protected override void OnDeactivate(bool close)
		{
			foreach (var archive in _archives)
				archive.Dispose();
			_archives.Clear();
			_installSettings.TemporaryFiles.Delete();
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
				_shell.TryClose();
			}
		}

		private void QueueZip(string path)
		{
			var archive = new ZipArchive(File.OpenRead(path), ZipArchiveMode.Read);
			_archives.Add(archive);
			TotalFiles += archive.Entries.Count;
		}

		private async Task ExtractArchives()
		{
			await Task.Run(() =>
			{
				foreach (var archive in _archives)
					ExtractArchive(archive);
			});
		}

		private void ExtractArchive(ZipArchive archive)
		{
			try
			{
				foreach (var entry in archive.Entries)
				{
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
				_shell.TryClose();
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
