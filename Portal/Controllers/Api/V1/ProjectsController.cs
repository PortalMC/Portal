using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Portal.Data;
using Portal.Models;

namespace Portal.Controllers.Api.V1
{
    [Produces("application/json")]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProjectsController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("")]
        public async Task<IActionResult> NewProject([FromBody] Project project)
        {
            if (project == null)
            {
                return BadRequest();
            }
            try
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                var safeUser = await _context.SafeUsers
                    .Include(s => s.Projects)
                    .SingleOrDefaultAsync(m => m.UserId == currentUser.Id);
                project.AccessRights = new List<AccessRight>();
                var createdProject = await _context.Projects.AddAsync(project);
                safeUser.Projects.Add(project);
                await TryUpdateModelAsync(safeUser, "", s => s.Projects);
                await _context.SaveChangesAsync();
                var root = new JObject
                {
                    {"success", new JValue(true)},
                    {
                        "data", new JObject
                        {
                            {"id", new JValue(createdProject.Entity.Id)}
                        }
                    }
                };
                return new OkObjectResult(root);
            }
            catch (Exception e)
            {
                var root = new JObject
                {
                    {"success", new JValue(false)},
                    {"message", new JValue(e.Message)}
                };
                return new OkObjectResult(root);
            }
        }

        [HttpPost("{uuid}/file/get")]
        public IActionResult Get(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var projectRoot = Path.Combine(new DirectoryInfo("projectsroot").FullName, uuid);
            var rawFile = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
            if (!rawFile.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Maybe directory traversal
                return NotFound();
            }
            if (!rawFile.Exists)
            {
                return NotFound();
            }
            var relativePath = rawFile.FullName.Substring(projectRoot.Length + 1).Replace("\\", "/");
            var extension = rawFile.Extension;
            if (extension.Length != 0)
            {
                extension = extension.Substring(1);
            }

            var retFile = new FileObject
            {
                Path = relativePath,
                Name = rawFile.Name,
                Extension = extension,
                Content = System.IO.File.ReadAllText(rawFile.FullName, Encoding.UTF8)
            };
            return new ObjectResult(retFile);
        }

        [HttpPost("{uuid}/file/edit")]
        public IActionResult Edit(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var projectRoot = Path.Combine(new DirectoryInfo("projectsroot").FullName, uuid);
            var rawFile = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
            if (!rawFile.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Maybe directory traversal
                return NotFound();
            }
            if (!rawFile.Exists)
            {
                return NotFound();
            }

            try
            {
                System.IO.File.WriteAllText(rawFile.FullName, file.Content, Encoding.UTF8);
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet("{uuid}/file/list")]
        public IActionResult List(string uuid)
        {
            if (uuid == null)
            {
                return BadRequest();
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var projectRoot = new DirectoryInfo(Path.Combine(new DirectoryInfo("projectsroot").FullName, uuid));
            if (!projectRoot.Exists)
            {
                return BadRequest();
            }
            var projectRootObj = new JArray();
            AddDirectories(projectRoot, projectRootObj);
            CheckBlacklistElemenets(projectRootObj);
            var root = new JArray
            {
                new JObject
                {
                    {"title", new JValue("!!ProjectName!!")},
                    {"folder", new JValue(true)},
                    {"expanded", new JValue(true)},
                    {"children", projectRootObj}
                }
            };
            return new OkObjectResult(root);
        }

        private static readonly string[] BlacklistElements =
        {
            ".gradle", "build", "eclipse", "gradle"
        };

        private static void CheckBlacklistElemenets(JArray array)
        {
            var deleting = array.Where(token => BlacklistElements.Contains(token["title"].Value<string>())).ToList();
            foreach (var token in deleting)
            {
                array.Remove(token);
            }
        }

        private static void AddDirectories(DirectoryInfo directory, JArray store)
        {
            var subDirectories = directory.GetDirectories().OrderByDescending(a => a.Name);
            foreach (var subDirectory in subDirectories)
            {
                var subDirectoryStore = new JArray();
                AddDirectories(subDirectory, subDirectoryStore);
                var directoryObject = new JObject
                {
                    {"title", new JValue(subDirectory.Name)},
                    {"folder", new JValue(true)},
                    {"children", subDirectoryStore}
                };
                store.Add(directoryObject);
            }
            var files = directory.GetFiles().OrderByDescending(a => a.Name);
            foreach (var file in files)
            {
                var fileObject = new JObject
                {
                    {"title", new JValue(file.Name)},
                };
                store.Add(fileObject);
            }
        }
    }
}