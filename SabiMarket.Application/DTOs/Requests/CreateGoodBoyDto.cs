using Microsoft.AspNetCore.Http;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateGoodBoyDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public List<string> MarketIds { get; set; } = new List<string>();
        public string ProfileImage { get; set; }

        public IFormFile GetProfileImage()
        {
            return string.IsNullOrEmpty(ProfileImage)
                ? null
                : FormFileHelper.Base64ToIFormFile(ProfileImage, $"{Guid.NewGuid()}.jpg");
        }
    }

    public class UpdateGoodBoyRequestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public List<string> MarketIds { get; set; } = new List<string>();
        public string ProfileImage { get; set; }

        public IFormFile GetProfileImage()
        {
            return string.IsNullOrEmpty(ProfileImage)
                ? null
                : FormFileHelper.Base64ToIFormFile(ProfileImage, $"{Guid.NewGuid()}.jpg");
        }
    }

    public static class FormFileHelper
    {

        public static IFormFile Base64ToIFormFile(string base64String, string fileName, string contentType = "image/jpeg")
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            // Remove data URI prefix if present
            if (base64String.Contains("base64,"))
            {
                base64String = base64String.Split(',')[1];
            }

            // Convert base64 string to byte array
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64String);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid Base64 string", nameof(base64String));
            }

            // Create a reusable MemoryStream
            var stream = new MemoryStream(fileBytes);

            // Create a MemoryStream that will not be closed when disposed
            var nonClosingStream = new NonClosingMemoryStream(stream);

            // Create IFormFile
            return new FormFile(nonClosingStream, 0, fileBytes.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

       /* public static IFormFile Base64ToIFormFile(string base64String, string fileName, string contentType = "image/jpeg")
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            // Remove data URI prefix if present
            if (base64String.Contains("base64,"))
            {
                base64String = base64String.Split(',')[1];
            }

            // Convert base64 string to byte array
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64String);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid Base64 string", nameof(base64String));
            }

            // Create memory stream from byte array
            using (var stream = new MemoryStream(fileBytes))
            {
                // Create IFormFile
                return new FormFile(stream, 0, fileBytes.Length, fileName, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };
            }
        }*/

        public static async Task<string> IFormFileToBase64(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }
    }

    public class NonClosingMemoryStream : MemoryStream
    {
        private readonly MemoryStream _innerStream;

        public NonClosingMemoryStream(MemoryStream innerStream)
        {
            _innerStream = innerStream;
        }

        public override void Close()
        {
            // Do nothing to prevent stream from closing
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            // Do not dispose the inner stream
        }
    }
}
