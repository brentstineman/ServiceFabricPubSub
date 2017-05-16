#region Assembly Microsoft.AspNetCore.Hosting.Abstractions, Version=1.1.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// C:\Users\odaibert\Source\Repos\service-fabric-dotnet-iot\src\packages\Microsoft.AspNetCore.Hosting.Abstractions.1.1.1\lib\net451\Microsoft.AspNetCore.Hosting.Abstractions.dll
#endregion

using System.Threading;

namespace Microsoft.AspNetCore.Hosting
{
    //
    // Summary:
    //     Allows consumers to perform cleanup during a graceful shutdown.
    public interface IApplicationLifetime
    {
        //
        // Summary:
        //     Triggered when the application host has fully started and is about to wait for
        //     a graceful shutdown.
        CancellationToken ApplicationStarted { get; }
        //
        // Summary:
        //     Triggered when the application host is performing a graceful shutdown. Requests
        //     may still be in flight. Shutdown will block until this event completes.
        CancellationToken ApplicationStopping { get; }
        //
        // Summary:
        //     Triggered when the application host is performing a graceful shutdown. All requests
        //     should be complete at this point. Shutdown will block until this event completes.
        CancellationToken ApplicationStopped { get; }

        //
        // Summary:
        //     Requests termination the current application.
        void StopApplication();
    }
}