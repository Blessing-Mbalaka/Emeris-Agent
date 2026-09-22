using EmerisAcademicSuccess.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace EmerisAcademicSuccess.Web.Models;

public sealed class DocumentUploadFormModel
{
    public IFormFile? File { get; set; }
    public DocumentCategory Category { get; set; }
}