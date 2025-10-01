using Domain.Entities.Enum;
using Microsoft.AspNetCore.Http;

namespace Domain.Payload.Request.Upload
{
    public class UploadRequest
    {
        public List<IFormFile> Files { get; set; }
        public FolderPath? Folder { get; set; }
        public string? CustomeFolder { get; set; }

    }
}
