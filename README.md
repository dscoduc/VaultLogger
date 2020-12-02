# VaultLogger
Socket Listener for accepting Vault Audit logging via Sockets with logging handled using NLog.  Log endpoints and filtering are configured using NLog.config file.

Executing the application without any startup arguments uses the default values displayed below.

### Optional Startup Arguments
  * -Address={string}
  
        IPv4 Address to listen for incoming connections (default = 127.0.0.1)
  * -Port={int}
  
        Port to listen for incoming connections [1024-65535] (default = 11000)
  * -ConnectionQueue={int}
  
        Number of incoming connections that can be queued for acceptance (default = 100)
  * -SocketBufferSize={int}

        Buffer size in kilobytes used to handle incoming socket data stream (default = 8192)

#### Example:
```
C:\VaultLogger.exe

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
  * https://nlog-project.org/config/?tab=targets
  * https://github.com/nlog/nlog/wiki/Filtering-log-messages
  * https://www.vaultproject.io/docs/audit/socket
