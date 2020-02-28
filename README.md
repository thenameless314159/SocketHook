# <img src="https://cdn3.iconfinder.com/data/icons/medicon/512/syringe_injection_drug_steroid-512.png" width="32" height="32"> SocketHook [![NuGet Badge](https://buildstats.info/nuget/SocketHook.Extensions)](https://www.nuget.org/packages/SocketHook.Extensions/1.0.0)

This application allow the user to redirect any call to the windows API [*connect*](https://docs.microsoft.com/en-us/windows/win32/api/winsock2/nf-winsock2-connect) of any process in order to redirect its connection to the configured local port.

![](https://i.ibb.co/dbpVg64/AJSr7-LQg-U0.png)

## Get started

The hook was meant to be **controlled by a REST API**, thanks to this, any kind of application can "*plug*" to it and send inject directives. A **WPF application using .NET Core 3.1** and the *generic hosting APIs* was made to show the potential of this architecture, you can see the code [on this repository ](https://github.com/thenameless314159/SocketHook/tree/master/samples/SocketHook.HostedWpfSample) or [**download the release directly**](https://github.com/thenameless314159/SocketHook/releases).

The API can either be started using regular **CLI args** or with a **json configuration file** on the working directory of the application (or with both). Here are two examples of json  config files :

```json
{
    "InjectToExe": "C:\\Users\\User\\Desktop\\SomeProgram.exe",
    "RedirectionPort": 8080,
    "RedirectedIps": [
      "xxx.xxx.xxx",
      "xxx.xxx.xxx"
    ]
}
```

```json
{
    "InjectToPId": 17881,
    "RedirectionPort": 8080,
    "RedirectedIps": [
      "xxx.xxx.xxx",
      "xxx.xxx.xxx"
    ]
}
```

Also, if no *InjectTo* args are being provided, the application will bind a **REST API** (default port is 80) to the localhost in order to allow any user to triggers **inject** or **createAndInject** directive to the application. A **killAll** directive is also provided in order to kill all the process that might have been created within this injector instance. The route are defined this way :

- PUT `http://127.0.0.1:80/api/createandinject?exePath=C:\\Users\\User\\Desktop\\SomeProgram.exe` 
	- with [*this json model*](https://github.com/thenameless314159/SocketHook/tree/master/src/SocketHook.API/Models/InjectionSettings.cs) in the request body.
- PUT `http://127.0.0.1:80/api/inject?pId=17881` 
	- also with [*this json model*](https://github.com/thenameless314159/SocketHook/tree/master/src/SocketHook.API/Models/InjectionSettings.cs) in the request body.
- DELETE `http://127.0.0.1:80/api/killall` for the **killAll** directive

## Go further

The WPF application takes advantage of the new .NET Core *generic hosting APIs*, therefore [a library](https://github.com/thenameless314159/SocketHook/tree/master/src/SocketHook.Extensions) that provides interaction with the hook REST API and extensions methods to register thoses services into the commonly used **`IServiceCollection`**. This library is also available [*on nuget at this address*](https://www.nuget.org/packages/SocketHook.Extensions/1.0.0).

The registration logic looks like this :

```csharp
	services.AddSocketHook(opt =>
	{
		opt.AddConfiguration(ctx.Configuration);
		opt.Configure(x =>
		{
			x.UseHookServiceFactory = true;
			x.OpenHookOnStartup = true;
			x.KillAllOnExit = true;
		});
	});
```

And this will registers at least an **`ISocketHookServiceFactory`** which will be responsible of creating **`ISocketHookService`** instances with their relative [**`InjectOptions`**](https://github.com/thenameless314159/SocketHook/tree/master/src/SocketHook.Extensions/Options/InjectOptions.cs).

The **`ISocketHookService`** provides 3 methods to interact with the hook API :

```csharp
    public interface ISocketHookService
    {
        ValueTask<bool> TryCreateAndInject(string exePath, CancellationToken token = default);
        ValueTask<bool> TryInject(int pId, CancellationToken token = default);

        /// <exception cref="HttpRequestException" />
        ValueTask KillAllInjectedProcesses(CancellationToken token = default);
    }
```

## Important

If you are experiencing any issue binding the HTTP server to your specified port please check this article from Microsoft : 
[https://docs.microsoft.com/fr-fr/dotnet/framework/wcf/feature-details/configuring-http-and-https?redirectedfrom=MSDN](https://docs.microsoft.com/fr-fr/dotnet/framework/wcf/feature-details/configuring-http-and-https?redirectedfrom=MSDN)
