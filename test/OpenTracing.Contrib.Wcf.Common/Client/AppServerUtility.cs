using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using OpenTracing.Contrib.Wcf.Common.BusinessLogic;

namespace OpenTracing.Contrib.Wcf.Common.Client
{
    public static class AppServerUtility
    {
        internal static readonly Dictionary<Type, string> ConfigurationAddresses = new Dictionary<Type, string>
        {
            { typeof(IReportService)     , "Reports" },
            { typeof(IThrowsService)     , "Throws" },
            { typeof(IExportService)     , "Exports" },
            { typeof(IImageService)      , "Images" },
        };

        private static readonly ConcurrentDictionary<Type, object> ChannelFactoryCache = new ConcurrentDictionary<Type, object>();

        public static T GetProxy<T>(string config, InstanceContext callback)
        {
            var channelFactory = (ChannelFactory<T>)ChannelFactoryCache.GetOrAdd(typeof(T), _ =>
            {
                var factory = callback != null
                    ? new DuplexChannelFactory<T>(callback, config)
                    : new ChannelFactory<T>(config);

                return factory;
            });

            return channelFactory.CreateChannel();
        }

        public static void Dispose(object proxy)
        {
            if (proxy == null) return;

            try
            {
                var communicationObject = (ICommunicationObject)proxy;
                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                }
                else
                {
                    try
                    {
                        communicationObject.Close();
                    }
                    catch
                    {
                        communicationObject.Abort();
                    }
                }
            }
            catch { /* Ignore */ }

            try
            {
                ((IDisposable)proxy).Dispose();
            }
            catch { /* Ignore */ }
        }

        public static T GetProxy<T>()
        {
            return GetProxy<T>(ConfigurationAddresses[typeof(T)], null);
        }

        #region |        Proxy Scope Requests          |

        public abstract class ProxyScopeBase<TInterface> : IDisposable
            where TInterface : class
        {
            private bool _disposed;

            private TInterface _proxy;
            public TInterface Proxy
            {
                get
                {
                    if (_disposed)
                        throw new ObjectDisposedException(this.GetType().FullName);
                    return _proxy ?? (_proxy = GetProxy<TInterface>());
                }
            }

            protected ProxyScopeBase()
            {
                _disposed = false;
            }

            ~ProxyScopeBase()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed) return;

                var proxy = _proxy;
                _disposed = true;
                _proxy = null;

                AppServerUtility.Dispose(proxy);
            }
        }

        public class ProxyScope<TInterface> : ProxyScopeBase<TInterface>
            where TInterface : class
        {
            public void MakeRequest(Action<TInterface> request)
            {
                request(Proxy);
            }
        }

        public class ProxyScope<TInterface, TResponse> : ProxyScopeBase<TInterface>
            where TInterface : class
        {
            public TResponse MakeRequest(Func<TInterface, TResponse> request)
            {
                return request(Proxy);
            }
        }

        public static ProxyScope<TInterface> CreateProxyScope<TInterface>()
            where TInterface : class
        {
            return new ProxyScope<TInterface>();
        }

        public static ProxyScope<TInterface, TResponse> CreateProxyScope<TInterface, TResponse>()
            where TInterface : class
        {
            return new ProxyScope<TInterface, TResponse>();
        }

        public static void MakeRequest<T>(Action<T> request)
            where T : class
        {
            using (var scope = CreateProxyScope<T>())
            {
                scope.MakeRequest(request);
            }
        }

        public static TResponse MakeRequest<TRequest, TResponse>(Func<TRequest, TResponse> request)
            where TRequest : class
        {
            using (var scope = CreateProxyScope<TRequest, TResponse>())
            {
                return scope.MakeRequest(request);
            }
        }

        #endregion
    }
}