using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessor.Web.Plugins.PostProcessor
{
    /// <summary>
    /// Provides methods to run a process asynchronously.
    /// </summary>
    /// <remarks>
    /// Copyright (c) 2013 James Manning
    /// 
    /// https://github.com/jamesmanning/RunProcessAsTask
    /// </remarks>
    public static class ProcessEx
    {
        /// <summary>
        /// Asynchronously runs the process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous process.
        /// </returns>
        public static Task<ProcessResults> RunAsync(string fileName, string arguments = null, CancellationToken cancellationToken = default(CancellationToken)) => RunAsync(new ProcessStartInfo(fileName, arguments), cancellationToken);

        /// <summary>
        /// Asynchronously runs the process.
        /// </summary>
        /// <param name="processStartInfo">The process start information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous process.
        /// </returns>
        public static async Task<ProcessResults> RunAsync(ProcessStartInfo processStartInfo, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Force some settings in the start info so we can capture the output
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            var processStartTime = new TaskCompletionSource<DateTime>();

            var standardOutput = new StringBuilder();
            var standardOutputResults = new TaskCompletionSource<string>();
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    standardOutput.AppendLine(args.Data);
                }
                else
                {
                    standardOutputResults.SetResult(standardOutput.ToString());
                }
            };

            var standardError = new StringBuilder();
            var standardErrorResults = new TaskCompletionSource<string>();
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    standardError.AppendLine(args.Data);
                }
                else
                {
                    standardErrorResults.SetResult(standardError.ToString());
                }
            };

            var tcs = new TaskCompletionSource<ProcessResults>();

            process.Exited += async (sender, args) =>
            {
                tcs.TrySetResult(new ProcessResults(process, await processStartTime.Task.ConfigureAwait(false), await standardOutputResults.Task.ConfigureAwait(false), await standardErrorResults.Task.ConfigureAwait(false)));
            };

            using (cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();

                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (InvalidOperationException)
                {
                    // The process has already exited
                }
            }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!process.Start())
                {
                    tcs.TrySetException(new InvalidOperationException("Failed to start process."));
                }

                processStartTime.SetResult(process.StartTime);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return await tcs.Task.ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Specifies the process results.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class ProcessResults : IDisposable
    {
        /// <summary>
        /// Gets the process.
        /// </summary>
        /// <value>
        /// The process.
        /// </value>
        public Process Process { get; }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public int ExitCode { get; }

        /// <summary>
        /// Gets the run time.
        /// </summary>
        /// <value>
        /// The run time.
        /// </value>
        public TimeSpan RunTime { get; }

        /// <summary>
        /// Gets the standard output.
        /// </summary>
        /// <value>
        /// The standard output.
        /// </value>
        public string StandardOutput { get; }

        /// <summary>
        /// Gets the standard error output.
        /// </summary>
        /// <value>
        /// The standard error output.
        /// </value>
        public string StandardError { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessResults"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="processStartTime">The process start time.</param>
        /// <param name="standardOutput">The standard output.</param>
        /// <param name="standardError">The standard error.</param>
        public ProcessResults(Process process, DateTime processStartTime, string standardOutput, string standardError)
        {
            this.Process = process;
            this.ExitCode = process.ExitCode;
            this.RunTime = process.ExitTime - processStartTime;
            this.StandardOutput = standardOutput;
            this.StandardError = standardError;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Process.Dispose();
        }
    }
}
