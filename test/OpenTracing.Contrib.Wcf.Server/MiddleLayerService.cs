using System;
using System.Collections.Generic;
using System.ServiceModel;
using Ninject;
using Ninject.Extensions.Wcf;
using OpenTracing.Contrib.Wcf.Common.BusinessLogic;

namespace OpenTracing.Contrib.Wcf.Server
{
    public class MiddleLayerService
    {
        private readonly List<ServiceHost> _hosts;
        private readonly IKernel _kernel;

        public MiddleLayerService()
        {
            _kernel = SetupDependencies();
            _hosts = new List<ServiceHost>();
        }

        private static IKernel SetupDependencies()
        {
            var kernel = new StandardKernel();

            return kernel;
        }

        public void OnStart()
        {
            AddHost<ReportService>("REPORS");
            AddHost<ThrowsService>("THROWS");
            AddHost<ExportService>("EXPORTS");
            AddHost<ImageService>("IMAGES");
        }

        private void AddHost<T>(string name, Action<ServiceHost> init = null)
        {
            var host = OpenHost<T>(name, init);
            _hosts.Add(host);
        }

        private ServiceHost OpenHost<T>(string name, Action<ServiceHost> init)
        {
            var host = _kernel.Get<NinjectServiceHost<T>>();
            host.Description.Name = name;
            init?.Invoke(host);
            host.Open();
            return host;
        }

        public void OnStop()
        {
            CloseHosts();
        }

        private void CloseHosts()
        {
            var hosts = new List<ServiceHost>(_hosts);
            _hosts.Clear();

            foreach (var host in hosts)
            {
                host.CloseOrAbort();
            }
        }
    }
}