# Logging

NuGet pakage [Logging.Memory](https://www.nuget.org/packages/Logging.Memory/)

## How to use

### 1. Enable Logging in Startup.cs

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            ...
            loggerFactory.MinimumLevel = LogLevel.Debug;
            loggerFactory.AddMemory();
            ...
        }
        
### 2. Now you can get log from static List everywhere
     
    var logList = MemoryLogger.LogList;

