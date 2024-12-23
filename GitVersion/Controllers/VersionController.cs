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
using Microsoft.Extensions.Configuration;
using GitVersion.Model;

namespace GitVersion.Controllers
{
    [Route("api")]
    [ApiController]
    public class VersionController : Controller
    {
        private readonly ILogger<VersionController> _logger;
        private readonly IConfiguration _configuration;

        public VersionController(ILogger<VersionController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("current-version")]
        public async Task<IActionResult> GetCurrentVersion([FromQuery] string moduleName)
        {
            try
            {
                string path = _configuration[$"{moduleName}:basePath"];
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
        public async Task<IActionResult> FetchVersion([FromQuery] string moduleName)
        {
            try
            {
                string path = _configuration[$"{moduleName}:basePath"];

                Command cmd = new Command(path, "fetch --all --tags");
                string tagsOutput = await cmd.ExecuteGitCommand();

                return Ok(new { Message = tagsOutput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("change-version")]
        public async Task<IActionResult> ChangeVersion([FromBody] ChangeVersionParam param)
        {
            try
            {
                string path = _configuration[$"{param.ModuleName}:basePath"];
                Command cmd = new Command(path);

                string cmdOutput = await cmd.ExecuteGitCommand($"checkout {param.Branch}", true);
                 
                if (param.Version != "development")
                {
                    string changeVerCmd = $"checkout tags/{param.Version}";
                    cmdOutput = await cmd.ExecuteGitCommand(changeVerCmd, true);
                } else
                {
                    cmdOutput = await cmd.ExecuteGitCommand("pull", true);
                }

                cmd.SetNewPath($"{path}{_configuration[param.ModuleName + ":deployPath"]}");
                await cmd.ExecuteBashCommand(_configuration[param.ModuleName + ":deployScript"]);


                return Ok(new { Message = cmdOutput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("versions")]
        public async Task<IActionResult> GetVersions([FromQuery] string moduleName)
        {
            try
            {
                string path = _configuration[$"{moduleName}:basePath"];
                Command cmd = new Command(path);

                await cmd.ExecuteGitCommand($"fetch --all --tags", true);
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
