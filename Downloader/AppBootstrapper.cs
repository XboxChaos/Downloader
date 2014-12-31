using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Downloader.ViewModels;

namespace Downloader
{
	class AppBootstrapper : BootstrapperBase
	{
		private CompositionContainer _container;

		public AppBootstrapper()
		{
			Initialize();
		}

		protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
		{
			DisplayRootViewFor<IShell>();
			Application.MainWindow.ResizeMode = ResizeMode.CanMinimize;
		}

		protected override void Configure()
		{
			_container = new CompositionContainer(
				new AggregateCatalog(
						AssemblySource.Instance.Select(x => new AssemblyCatalog(x))
					)
				);

			var batch = new CompositionBatch();
			batch.AddExportedValue<IWindowManager>(new WindowManager());
			batch.AddExportedValue<IEventAggregator>(new EventAggregator());
			batch.AddExportedValue(_container);
			_container.Compose(batch);
		}

		protected override object GetInstance(Type serviceType, string key)
		{
			var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
			var exports = _container.GetExportedValues<object>(contract);

			var export = exports.FirstOrDefault();
			if (export != null)
				return export;

			throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
		}

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
		}

		protected override void BuildUp(object instance)
		{
			_container.SatisfyImportsOnce(instance);
		}
	}
}
