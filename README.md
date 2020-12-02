# VaultLogger
Socket Listener for accepting Vault Audit logging via Sockets with logging handled using NLog.  Log endpoints (https://nlog-project.org/config/?tab=targets) and filtering (https://github.com/nlog/nlog/wiki/Filtering-log-messages) is configured using NLog.config file.

See https://www.vaultproject.io/docs/audit/socket for details on configuring Vault Auditing.

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

### Example:  
```
C:\VaultLogger.exe -Address=192.168.1.10 -Port=9090

Listening using the following settings:
-Address=192.168.1.10
-Port=9090
-ConnectionQueue=100
-SocketBufferSize=8192

[addb9773-fe15-40e0-bd43-99cb16e520eb] New socket connection established
[addb9773-fe15-40e0-bd43-99cb16e520eb] Socket connection closed
[addb9773-fe15-40e0-bd43-99cb16e520eb] Sending 1 bytes to logger [AuditLog]
```
