using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Portal.Extensions;
using Portal.Models;
using Portal.Utils;

namespace Portal.Controllers.Api.V1
{
    public partial class ProjectsController
    {
        [HttpPost("{uuid}/file/get")]
        [Authorize]
        public async Task<IActionResult> Get(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
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
        [Authorize]
        public async Task<IActionResult> Edit(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
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
                await System.IO.File.WriteAllTextAsync(rawFile.FullName, file.Content, Encoding.UTF8);
                await _context.UpdateProjectUpdatedAtAsync(uuid);
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost("{uuid}/file/move")]
        [Authorize]
        public async Task<IActionResult> Move(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
            if (file.IsDirectory)
            {
                var rawDirectory = new DirectoryInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
                var rawDirectoryNew = new DirectoryInfo(Path.Combine(projectRoot, Path.Combine(file.NewPath.Split('/'))));
                if (!rawDirectory.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase) || !rawDirectoryNew.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    // Maybe directory traversal
                    return NotFound();
                }
                if (!rawDirectory.Exists)
                {
                    return NotFound();
                }
                if (rawDirectoryNew.Exists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Destination path is already exist.");
                }
                try
                {
                    rawDirectory.MoveTo(rawDirectoryNew.FullName);
                    await _context.UpdateProjectUpdatedAtAsync(uuid);
                    return NoContent();
                }
                catch (Exception e)
                {
                    StatusCode(StatusCodes.Status500InternalServerError, "An error occueerd while moving project file.");
                    Console.WriteLine(e);
                    throw;
                }
            }
            // File
            var rawFile = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
            var rawFileNew = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.NewPath.Split('/'))));
            if (!rawFile.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase) || !rawFileNew.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Maybe directory traversal
                return NotFound();
            }
            if (!rawFile.Exists)
            {
                return NotFound();
            }
            if (rawFileNew.Exists)
            {
                return StatusCode(StatusCodes.Status409Conflict, "Destination path is already exist.");
            }

            try
            {
                rawFile.MoveTo(rawFileNew.FullName);
                await _context.UpdateProjectUpdatedAtAsync(uuid);
                return NoContent();
            }
            catch (Exception e)
            {
                StatusCode(StatusCodes.Status500InternalServerError, "An error occueerd while moving project file.");
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost("{uuid}/file/create")]
        [Authorize]
        public async Task<IActionResult> Create(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
            if (file.IsDirectory)
            {
                var rawDirectory = new DirectoryInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
                if (!rawDirectory.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    // Maybe directory traversal
                    return NotFound();
                }
                if (rawDirectory.Exists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Directory is already exist.");
                }
                try
                {
                    rawDirectory.Create();
                    await _context.UpdateProjectUpdatedAtAsync(uuid);
                    return NoContent();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            var rawFile = new FileInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
            if (!rawFile.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Maybe directory traversal
                return NotFound();
            }
            if (rawFile.Exists)
            {
                return StatusCode(StatusCodes.Status409Conflict, "File is already exist.");
            }
            try
            {
                // TODO: Snippet support
                rawFile.Create().Close();
                await _context.UpdateProjectUpdatedAtAsync(uuid);
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost("{uuid}/file/delete")]
        [Authorize]
        public async Task<IActionResult> Delete(string uuid, [FromBody] FileObject file)
        {
            if (uuid == null || file == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var canAccess = await _context.CanAccessToProjectAsync(_userManager.GetUserId(HttpContext.User), uuid);
            if (!canAccess)
            {
                return NotFound();
            }
            var projectRoot = Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid);
            if (file.IsDirectory)
            {
                var rawDirectory = new DirectoryInfo(Path.Combine(projectRoot, Path.Combine(file.Path.Split('/'))));
                if (!rawDirectory.FullName.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    // Maybe directory traversal
                    return NotFound();
                }
                if (!rawDirectory.Exists)
                {
                    return NotFound();
                }
                try
                {
                    rawDirectory.Delete(true);
                    await _context.UpdateProjectUpdatedAtAsync(uuid);
                    return NoContent();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
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
                rawFile.Delete();
                await _context.UpdateProjectUpdatedAtAsync(uuid);
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet("{uuid}/file/list")]
        [Authorize]
        public async Task<IActionResult> FileList(string uuid)
        {
            if (uuid == null)
            {
                return BadRequest();
            }
            if (!Util.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            var userId = _userManager.GetUserId(HttpContext.User);
            var result = await _context.CanAccessToProjectWithProjectAsync(userId, uuid);
            if (!result.canAccess)
            {
                return NotFound();
            }
            var projectRoot = new DirectoryInfo(Path.Combine(_storageSetting.GetProjectStorageSetting().GetRootDirectory().FullName, uuid));
            if (!projectRoot.Exists)
            {
                return BadRequest();
            }
            var projectRootObj = new JArray();
            var id = 0;
            AddDirectories(projectRoot, projectRootObj, ref id);
            CheckBlacklistElemenets(projectRootObj);
            var root = new JArray
            {
                new JObject
                {
                    {"title", new JValue(result.project.Name)},
                    {"key", new JValue($"key_{++id}")},
                    {"folder", new JValue(true)},
                    {"expanded", new JValue(true)},
                    {"children", projectRootObj}
                }
            };
            return new OkObjectResult(root);
        }
    }
}