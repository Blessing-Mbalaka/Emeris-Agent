using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Models;

public sealed record DocumentUploadRequest(
    string FileName,
    string ContentType,
    DocumentCategory Category,
    byte[] ContentBytes);