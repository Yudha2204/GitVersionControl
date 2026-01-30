using System.Diagnostics;
using System.Threading.Tasks;
using System;
using GitVersion.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace GitVersion
{
    public class Command
    {
        private string _path { get; set; }
        private string _commandString { get; set; }
        private bool _ignoreErrorOutput { get; set; } = false;

        public Command(string path, string command, bool ignoreErrorOutput = false) { 
            _path = path;
            _commandString = command;
            _ignoreErrorOutput = ignoreErrorOutput;
        }

        public Command(string path)
        {
            _path = path;
        }

        public void SetNewPath(string path)
        {
            _path = path;
        }

        public async Task<string> ExecuteGitCommand(string arguments, bool ignoreErrorOutput = false)
        {
            _commandString = arguments;
            _ignoreErrorOutput = ignoreErrorOutput;
            return await this.BaseExecutor("git");
        }

        public async Task<string> ExecuteGitCommand(bool ignoreErrorOutput = false)
        {
            _ignoreErrorOutput = ignoreErrorOutput;
            return await this.BaseExecutor("git");
        }

        public async Task<string> ExecuteBashCommand(string arguments, bool ignoreErrorOutput = false)
        {
            _commandString = arguments;
            _ignoreErrorOutput = ignoreErrorOutput;
            return await this.BaseExecutor();
        }

        public async Task<string> ExecuteBashCommand(bool ignoreErrorOutput = false)
        {
            _ignoreErrorOutput = ignoreErrorOutput;
            return await this.BaseExecutor();
        }

        public static List<Commit> FormatGitLogOutputToJsonArray(string gitLogOutput)
        {
            gitLogOutput = gitLogOutput.Replace("\n", "\\n")
                                       .Replace("\r", "\\r");
            var lines = gitLogOutput.Split(new string[] { ";\\n", ";" }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrEmpty(x));

            var commits = new List<Commit>();

            foreach (var line in lines)
            {
                try
                {
                    var commit = JsonSerializer.Deserialize<Commit>(line);
                    commits.Add(commit);
                }
                catch (JsonException ex)
                {
                    throw new Exception($"Error parsing line: {line}\n{ex.Message}");
                }
            }
            return commits;
        }

        private async Task<string> BaseExecutor(string fileName = "/bin/bash")
        {
            if (string.IsNullOrEmpty(_path))
                throw new ArgumentNullException(nameof(_path));

            if (string.IsNullOrEmpty(_commandString))
                throw new ArgumentNullException(nameof(_commandString));

            var cmd = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = _path,
                Arguments = this._commandString,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(cmd))
            {
                if (process == null)
                {
                    return null;
                }

                string output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                string error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

                await process.WaitForExitAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(error))
                {
                    if (_ignoreErrorOutput)
                    {
                        return error.Trim();
                    }

                    throw new Exception($"Git error: {error}");
                }

                return output.Trim();
            }
        }
    }
}
