using GitVersion.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace GitVersion.Controllers
{
    [Route("api")]
    [ApiController]
    public class VersionController : Controller
    {
        private readonly ILogger<VersionController> _logger;

        public VersionController(ILogger<VersionController> logger)
        {
            _logger = logger;
        }

        [HttpGet("current-version")]
        public async Task<IActionResult> GetCurrentVersion([FromQuery] string path)
        {
            try
            {
                Command cmd = new Command(path, "describe");
                string tagsOutput = await cmd.ExecuteGitCommand();

                if (string.IsNullOrEmpty(tagsOutput))
                {
                    return Ok(new { Version = "v1.9.9" });
                }

                return Ok(new { Version = tagsOutput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("fetch-version")]
        public async Task<IActionResult> FetchVersion([FromQuery] string path)
        {
            try
            {
                Command cmd = new Command(path, "fetch --all --tags");
                string tagsOutput = await cmd.ExecuteGitCommand();

                return Ok(new { Message = tagsOutput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("change-version")]
        public async Task<IActionResult> ChangeVersion([FromQuery] string path, [FromQuery] string version, [FromQuery] string branch = "master")
        {
            try
            {
                Command cmd = new Command(path);

                await cmd.ExecuteGitCommand($"fetch --all --tags", true);
                string cmdOutput = await cmd.ExecuteGitCommand($"checkout {branch}", true);
                 
                if (version != "development")
                {
                    string changeVerCmd = $"checkout tags/{version}";
                    cmdOutput = await cmd.ExecuteGitCommand(changeVerCmd, true);
                } else
                {
                    cmdOutput = await cmd.ExecuteGitCommand("pull", true);
                }


                return Ok(new { Message = cmdOutput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("versions")]
        public async Task<IActionResult> GetVersions([FromQuery] string path, [FromQuery] int count = 1)
        {
            try
            {
                Command cmd = new Command(path);

                string tagCommand = $"tag --sort=-refname";
                string tagsOutput = await cmd.ExecuteGitCommand(tagCommand);

                if (string.IsNullOrEmpty(tagsOutput))
                {
                    return Ok(new { Message = "No tags version found." });
                }

                string[] tags = tagsOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                var versions = new List<object>();

                // 2. Get commit details for each tag

                for (int i = 0; i < tags.Length; i++)
                {
                    string tag = tags[i];
                    string logCommand = null;
                    if (i == tags.Length - 1)
                    {
                        logCommand = $"log --pretty=format:\"{{\\\"Date\\\":\\\"%ci\\\",\\\"Message\\\":\\\"%B\\\"}}\" {tag}";
                    } else
                    {
                        logCommand = $"log {tags[i + 1]}..{tag} --pretty=format:\"{{\\\"Date\\\":\\\"%ci\\\",\\\"Message\\\":\\\"%B\\\"}}\";";
                    }

                    string logOutput = await cmd.ExecuteGitCommand(logCommand);

                    var commitDetails = Command.FormatGitLogOutputToJsonArray(logOutput);
                    versions.Add(new
                    {
                        Version = tag,
                        VersionDate = DateTime.Parse(commitDetails.First().Date, System.Globalization.CultureInfo.InvariantCulture),
                        Changes = commitDetails
                    });
                }
                return Ok(versions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
