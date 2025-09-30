using Domain.Payload.Request.Upload;
using Domain.Share.Util;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/uploads")]
    public class UploadController(ILogger<UploadController> _logger) : Controller
    {
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple([FromForm] UploadRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
                return BadRequest("No files uploaded.");

            try
            {
                string folderPath;

                if (request.Folder.HasValue)
                {
                    folderPath = CommonUtil.GetFolderPath(request.Folder.Value);
                }
                else if (!string.IsNullOrEmpty(request.CustomeFolder))
                {
                    if (request.CustomeFolder.Contains("..") || Path.IsPathRooted(request.CustomeFolder))
                        return BadRequest("Invalid custom path.");

                    folderPath = request.CustomeFolder;
                }
                else
                {
                    return BadRequest("Folder not specified.");
                }

                var uploadedFiles = new List<string>();

                foreach (var file in request.Files)
                {
                    if (file.Length > 0)
                    {
                        var filePath = await CommonUtil.SaveImageToRootAsync(file, folderPath);
                        uploadedFiles.Add(filePath);
                    }
                }

                return Ok(new { Files = uploadedFiles });
            }
            catch (Exception ex)
            {
                _logger.LogError("[UploadMultiple API]" + ex.Message, ex.StackTrace);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
