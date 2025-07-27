using cxp.Models;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace cxp.Services;

public class GitService
{
    private static string GitExecutable => ConfigurationService.Settings.Git.ExecutablePath;

    public static async Task<GitResult> RunGitCommandAsync(string[] args)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GitExecutable,
                    Arguments = string.Join(" ", args.Select(EscapeArgument)),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                    stdout.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.Error.WriteLine(e.Data);
                    stderr.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return new GitResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = stdout.ToString(),
                StandardError = stderr.ToString()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error running git command: {ex.Message}");
            return new GitResult
            {
                ExitCode = -1,
                StandardOutput = "",
                StandardError = ex.Message
            };
        }
    }

    public static async Task<GitDiffStats?> GetCommitDiffStatsAsync()
    {
        try
        {
            var statsProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GitExecutable,
                    Arguments = "diff --stat HEAD~1",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            statsProcess.Start();
            var statsOutput = await statsProcess.StandardOutput.ReadToEndAsync();
            await statsProcess.WaitForExitAsync();

            var statusProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GitExecutable,
                    Arguments = "diff --name-status HEAD~1",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            statusProcess.Start();
            var statusOutput = await statusProcess.StandardOutput.ReadToEndAsync();
            await statusProcess.WaitForExitAsync();

            if (statsProcess.ExitCode != 0 || statusProcess.ExitCode != 0)
            {
                return null;
            }

            return ParseDiffStats(statsOutput, statusOutput);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<string?> GetLastCommitMessageAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GitExecutable,
                    Arguments = "log -1 --pretty=%B",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    private static string EscapeArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
            return "\"\"";

        if (!arg.Contains(' ') && !arg.Contains('\t') && !arg.Contains('\n') && !arg.Contains('\"'))
            return arg;

        return "\"" + arg.Replace("\"", "\\\"") + "\"";
    }

    private static GitDiffStats ParseDiffStats(string statsOutput, string statusOutput)
    {
        var stats = new GitDiffStats();

        // Parse stats output like: "2 files changed, 15 insertions(+), 3 deletions(-)"
        var numbers = Regex.Matches(statsOutput, @"\d+").Cast<Match>().Select(m => int.Parse(m.Value)).ToArray();

        if (numbers.Length >= 1)
        {
            var totalChanges = numbers.Sum();
            stats.TotalChanges = totalChanges;
        }

        var deletionsMatch = Regex.Match(statsOutput, @"(\d+)\s+deletions");
        if (deletionsMatch.Success)
        {
            stats.Deletions = int.Parse(deletionsMatch.Groups[1].Value);
        }

        // Parse status output to count added and deleted files
        var statusLines = statusOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        stats.FilesAdded = statusLines.Count(line => line.StartsWith("A\t"));
        stats.FilesDeleted = statusLines.Count(line => line.StartsWith("D\t"));

        return stats;
    }
}