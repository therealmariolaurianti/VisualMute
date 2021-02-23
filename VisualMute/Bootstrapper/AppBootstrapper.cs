using System;
using System.Collections.Generic;
using System.Windows;
using Bootstrap.Ninject;
using Caliburn.Micro;
using Ninject;
using VisualMute.Shell;

namespace VisualMute.Bootstrapper
{
    public class AppBootstrapper : BootstrapperBase
    {
        private StandardKernel _kernel;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                _kernel = new StandardKernel();

                _kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
                _kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();

                Bootstrap.Bootstrapper
                    .Including.Assembly(typeof(AppBootstrapper).Assembly)
                    .With.Ninject()
                    .WithContainer(_kernel)
                    .Start();

                DisplayRootViewFor<ShellViewModel>();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _kernel.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _kernel.Inject(instance);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            _kernel.Dispose();
            base.OnExit(sender, e);
        }

        protected override object GetInstance(Type service, string key)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return _kernel.Get(service);
        }
    }
}