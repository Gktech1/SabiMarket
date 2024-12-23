using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QRCoder;

namespace SabiMarket.Infrastructure.Utilities
{
    public class QRCodeGeneratorService
    {
        public byte[] GenerateQRCode<T>(T data)
        {

            // Serialize the data into JSON format
            var qrData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true // Makes the JSON readable
            });

            // Generate QR Code
            using var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);

            //using var qrCode = new QRCode(qrCodeData);
            //using Bitmap qrCodeImage = qrCode.GetGraphic(20); // 20 is the pixel size

            // Convert QR Code to byte array
            using var ms = new MemoryStream();
            // qrCodeImage.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
