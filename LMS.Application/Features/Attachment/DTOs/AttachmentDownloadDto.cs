namespace LMS.Application.Features.Attachment.DTOs
{
    public class AttachmentDownloadDto
    {
        public AttachmentDownloadDto(Stream content, string fileName, string contentType, long contentLength)
        {
            Content = content;
            FileName = fileName;
            ContentType = contentType;
            ContentLength = contentLength;
        }

        public Stream Content { get; }
        public string FileName { get; }
        public string ContentType { get; }
        public long ContentLength { get; }
    }
}
