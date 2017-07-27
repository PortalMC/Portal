using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.Models;

namespace Portal.Controllers.Api.V1
{
    [Produces("application/json")]
    [Route("api/v1/file")]
    [Authorize]
    public class FileController : Controller
    {
        [HttpPost("{uuid}/get")]
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

            var retFile = new FileObject
            {
                Path = relativePath,
                Name = rawFile.Name,
                Extension = rawFile.Extension.Substring(1),
                Content = System.IO.File.ReadAllText(rawFile.FullName, Encoding.UTF8)
            };
            return new ObjectResult(retFile);
        }
        
        [HttpPost("{uuid}/edit")]
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
    }
}