# VaultLogger

## Summary
Socket Listener for accepting Vault Audit logging via Sockets.  This solution provides the option to use a local socket listener that can receive the Vault audit events and then leverage NLog to manage/rotate/prune the Vault audit log files.

Vault does not include any native way to manage/rotate/prune log files and relies on the operating system or other logging services to handle these services.  This is fine for Linux based systems but Windows doesn't include the necessary services.  For example, using a log file for auditing on a Windows system presents a challenge as there isn't a SigHup comparable option for forcing Vault to disconnect/reconnect to the log file.

### Running as a Service
When running Vault on a Windows platform, it is recommended to use Nssm for configuring Vault to run as a service.  Using Nssm, it is also possible to run VaultLogger as a service and make it a dependent service for Vault.  Once installed as a service, VaultLogger can be running in parallel to the Vault service and accept/process audit log input via the Vault Socket logging option. 

### Optional Startup Arguments
Executing the application without any startup arguments uses the default values displayed below.

  * -Address={string}
  
        IPv4 Address to listen for incoming connections (default = 127.0.0.1)
        NOTE: Must be a specific address; 0.0.0.0 is not supported.
  * -Port={int}
  
        Port to listen for incoming connections [1024-65535] (default = 11000)
  * -ConnectionQueue={int}
  
        Number of incoming connections that can be queued for acceptance (default = 100)
  * -SocketBufferSize={int}

        Buffer size in kilobytes used in handle incoming socket data stream (default = 8192)

#### Example:
```
C:\VaultLogger.exe

Starting up...
Listening using the following settings:
-Address=127.0.0.1
-Port=11000
-ConnectionQueue=100
-SocketBufferSize=8192

[5ea327c1-9e92-4416-a3e0-b6e1a7951dfb] New socket connection established
[5ea327c1-9e92-4416-a3e0-b6e1a7951dfb] Socket connection closed
[5ea327c1-9e92-4416-a3e0-b6e1a7951dfb] Sending 100 bytes to logger [AuditLog]
```

#### Example:
```
C:\VaultLogger.exe -Address=192.168.1.10 -Port=9090 -ConnectionQueue=10 -SocketBufferSize=4096

Starting up...
Listening using the following settings:
-Address=192.168.1.10
-Port=9090
-ConnectionQueue=10
-SocketBufferSize=4096

[addb9773-fe15-40e0-bd43-99cb16e520eb] New socket connection established
[addb9773-fe15-40e0-bd43-99cb16e520eb] Socket connection closed
[addb9773-fe15-40e0-bd43-99cb16e520eb] Sending 100 bytes to logger [AuditLog]
```

#### Additional Information
  * https://www.vaultproject.io/docs/audit/socket
  * https://nlog-project.org/config/?tab=targets
  * https://github.com/nlog/nlog/wiki/Filtering-log-messages
  * https://nssm.cc/
