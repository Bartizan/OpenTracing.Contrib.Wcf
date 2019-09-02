# OpenTracing Wcf Instrumentation

OpenTracing instrumentation for Wcf.

## Installation

Install the [NuGet package](https://www.nuget.org/packages/OpenTracing.Contrib.Wcf//):

    Install-Package OpenTracing.Contrib.Wcf


## Usage

### Server

To intercept service response, you can use attribute [ServerTracingInterceptorAttribute] or setting in app.config

#### Use attribute [ServerTracingInterceptorAttribute]

```csharp
using OpenTracing.Contrib.Wcf.Interceptors;

[ServiceContract]
public interface IFooService
{
	[OperationContract]
	void Foo();
}

[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
[ServerTracingInterceptor]
public class MyFooService : IFooService
{
	public void Foo()
	{

	}
}

class Program
{
	static void Main(string[] args)
	{
		var tracer = CreateYourTracer();
        GlobalTracer.Register(tracer);
		//...
	}
}
```

#### Use setting in app.config

```csharp
using OpenTracing.Contrib.Wcf.Interceptors;

[ServiceContract]
public interface IBazService
{
	[OperationContract]
	void Baz();
}

[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
public class MyBazService : IBazService
{
	public void Baz()
	{

	}
}

class Program
{
	static void Main(string[] args)
	{
		var tracer = CreateYourTracer();
        GlobalTracer.Register(tracer);
		//...
	}
}
```

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <system.serviceModel>
    	<bindings>
            <netTcpBinding>
                <binding name="Baz" ...>
                </binding>
            </netTcpBinding>
        </bindings>
    	<services>
    		<!-- in behaviorConfiguration set behavior for SOAP service -->
            <service behaviorConfiguration="withServerTracingInterceptor" name="your-namespace.MyBazService">
                <endpoint address="net.tcp://localhost:5001/BazEndPoint/Baz" binding="netTcpBinding"
                          bindingConfiguration="Baz" contract="your-namespace.IMyBazService" name="Baz" />
            </service>
        </services>

        <extensions>
            <behaviorExtensions>
            	<!-- connect extension -->
                <add name="serverTracingInterceptor" type="OpenTracing.Contrib.Wcf.Interceptors.ServerTracingExtensionElement, OpenTracing.Contrib.Wcf, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null"/>
            </behaviorExtensions>
        </extensions>

        <behaviors>
            <serviceBehaviors>
                <behavior name="withServerTracingInterceptor">
                	<!-- connect extension to service -->
                    <serverTracingInterceptor />
                </behavior>
            </serviceBehaviors>
        </behaviors>

    </system.serviceModel>
</configuration>

```

### Client

To intercept client requests, you can use 'ClientTracingBehavior' or setting in app.config

#### Use 'ClientTracingBehavior'

```csharp
using OpenTracing.Contrib.Wcf.Interceptors;

class Program
{
	static void Main(string[] args)
	{
		var tracer = CreateYourTracer();
        GlobalTracer.Register(tracer);
		
		var config = "Foo";
		var factory = new ChannelFactory<IFooService>(config);
		factory.Endpoint.EndpointBehaviors.Add(new Interceptors.ClientTracingBehavior());
		var proxy = (IFooService)channelFactory.CreateChannel();
		try
		{
			proxy.Foo();
		}
		finally
		{
			((IDisposable)proxy).Dispose();
		}
	}
}
```

### Use setting in app.config

```csharp
using OpenTracing.Contrib.Wcf.Interceptors;

class Program
{
	static void Main(string[] args)
	{
		var tracer = CreateYourTracer();
        GlobalTracer.Register(tracer);

		var config = "Baz";
		var factory = new ChannelFactory<IBazService>(config);
		var proxy = (IBazService)channelFactory.CreateChannel();
		try
		{
			proxy.Baz();
		}
		finally
		{
			((IDisposable)proxy).Dispose();
		}
	}
}
```

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <system.serviceModel>
		<bindings>
            <netTcpBinding>
                <binding name="Baz" ...>
                </binding>
            </netTcpBinding>
        </bindings>

    	<client>
    		<!-- in behaviorConfiguration set behavior for SOAP service -->
            <endpoint address="net.tcp://localhost:5001/ReportingEndPoint/Reporting" behaviorConfiguration="withClientTracingInterceptor" binding="netTcpBinding"
                      bindingConfiguration="Baz" contract="your-namespace.IMyBazService" name="Baz" />
        </client>

        <extensions>
            <behaviorExtensions>
            	<!-- connect extension -->
                <add name="clientTracingInterceptor" type="OpenTracing.Contrib.Wcf.Interceptors.ClientTracingExtensionElement, OpenTracing.Contrib.Wcf, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null"/>
            </behaviorExtensions>
        </extensions>

        <behaviors>
            <endpointBehaviors>
                <behavior name="withClientTracingInterceptor">
                	<!-- connect extension to client -->
                    <clientTracingInterceptor />
                </behavior>
            </endpointBehaviors>
        </behaviors>
    </system.serviceModel>
</configuration>
```
