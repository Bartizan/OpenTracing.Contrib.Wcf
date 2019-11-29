using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
[assembly: AssemblyCompany("Medialogia®")]
[assembly: AssemblyProduct("OpenTracing WCF Instrumentation")]
[assembly: AssemblyCopyright("Copyright (c) 2019 Medialogia. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.2.1.0")]

[assembly: ComVisible(false)]

[assembly: NeutralResourcesLanguage("en")]