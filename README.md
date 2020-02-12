# <img src="https://cdn3.iconfinder.com/data/icons/medicon/512/syringe_injection_drug_steroid-512.png" width="32" height="32"> SocketHook

This application allow the user to redirect any call to the windows API [*connect*](https://docs.microsoft.com/en-us/windows/win32/api/winsock2/nf-winsock2-connect) of any process in order to redirect its connection to the configured local port.

## Get started

Thanks to the recent update, you can either start the application with regular **CLI args** or with a **json configuration file** on the working directory of the application (or with both). Here are two examples of json  config files :

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

- GET `http://127.0.0.1:80/api/createandinject?exePath=C:\\Users\\User\\Desktop\\SomeProgram.exe` 
	- with [*this json model*](https://github.com/thenameless314159/SocketHook/tree/master/src/SocketHook.API/Models/InjectionSettings.cs) in the request body.
- GET `http://127.0.0.1:80/api/inject?pId=17881` 
	- also with [*this json model*](https://github.com/thenameless314159/SocketHook/tree/master/src/SocketHook.API/Models/InjectionSettings.cs) in the request body.
- DELETE `http://127.0.0.1:80/api/killall` for the **killAll** directive

## Important

If you are experiencing any issue binding the HTTP server to your specified port please check this article from Microsoft : 
https://docs.microsoft.com/fr-fr/dotnet/framework/wcf/feature-details/configuring-http-and-https?redirectedfrom=MSDN
