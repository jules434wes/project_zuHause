using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using ImageSharpColor = SixLabors.ImageSharp.Color;
using PdfColor = iText.Kernel.Colors.Color;

namespace zuHause.Tests.TestHelpers
{
    /// <summary>
    /// 真實檔案建立器 - 使用 ImageSharp 和 iText7 建立真實的圖片和 PDF 檔案
    /// 絕對不使用 Mock 檔案，確保測試檔案與真實檔案完全一致
    /// </summary>
    public static class RealFileBuilder
    {
        /// <summary>
        /// 建立真實的 JPEG 圖片檔案
        /// </summary>
        /// <param name="width">圖片寬度 (預設 800px)</param>
        /// <param name="height">圖片高度 (預設 600px)</param>
        /// <param name="quality">JPEG 品質 0-100 (預設 85)</param>
        /// <param name="fileName">檔案名稱 (預設 test_property.jpg)</param>
        /// <returns>真實的 IFormFile 圖片檔案</returns>
        public static IFormFile CreateRealJpegImage(
            int width = 800, 
            int height = 600, 
            int quality = 85, 
            string fileName = "test_property.jpg")
        {
            using var image = new Image<Rgb24>(width, height);
            
            // 建立真實的房源圖片內容 - 使用像素操作
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new Rgb24(173, 216, 230); // 淺藍色
                }
            }
            
            var stream = new MemoryStream();
            image.SaveAsJpeg(stream, new JpegEncoder { Quality = quality });
            stream.Position = 0;
            
            return new FormFile(stream, 0, stream.Length, "PropertyImage", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        /// <summary>
        /// 建立真實的 PNG 圖片檔案
        /// </summary>
        /// <param name="width">圖片寬度</param>
        /// <param name="height">圖片高度</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>真實的 IFormFile PNG 檔案</returns>
        public static IFormFile CreateRealPngImage(
            int width = 800, 
            int height = 600, 
            string fileName = "test_property.png")
        {
            using var image = new Image<Rgba32>(width, height);
            
            // 建立透明背景的房源圖示 - 使用像素操作
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new Rgba32(0, 0, 0, 0); // 透明
                }
            }
            
            var stream = new MemoryStream();
            image.SaveAsPng(stream);
            stream.Position = 0;
            
            return new FormFile(stream, 0, stream.Length, "PropertyImage", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        /// <summary>
        /// 建立真實的 PDF 文件檔案
        /// </summary>
        /// <param name="content">PDF 內容 (預設為房產證明內容)</param>
        /// <param name="fileName">檔案名稱 (預設 property_proof.pdf)</param>
        /// <returns>真實的 IFormFile PDF 檔案</returns>
        public static IFormFile CreateRealPdfDocument(
            string content = "房產所有權狀\n\n本文件證明以下房產之所有權...\n\n測試用途房產證明文件", 
            string fileName = "property_proof.pdf")
        {
            var stream = new MemoryStream();
            
            using (var writer = new PdfWriter(stream))
            {
                writer.SetCloseStream(false); // 重要：防止 PdfWriter 關閉 stream
                
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);
                
                // 新增標題
                var title = new Paragraph("房產所有權狀證明文件")
                    .SetFontSize(20)
                    .SetBold()
                    .SetFontColor(ColorConstants.DARK_GRAY);
                document.Add(title);
                
                // 新增分隔線
                document.Add(new Paragraph("───────────────────────────────────────"));
                
                // 新增內容
                var contentParagraph = new Paragraph(content)
                    .SetFontSize(12)
                    .SetMarginTop(20);
                document.Add(contentParagraph);
                
                // 新增時間戳記
                var timestamp = new Paragraph($"\n文件建立時間: {DateTime.Now:yyyy年MM月dd日 HH:mm:ss}")
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetMarginTop(30);
                document.Add(timestamp);
                
                // 新增測試標記
                var testMark = new Paragraph("※ 此為測試用途文件 ※")
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.RED)
                    .SetBold()
                    .SetMarginTop(20);
                document.Add(testMark);
            }
            
            stream.Position = 0;
            
            return new FormFile(stream, 0, stream.Length, "PropertyProofDocument", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }

        /// <summary>
        /// 建立大尺寸圖片檔案 - 用於測試檔案大小限制
        /// </summary>
        /// <param name="sizeMB">目標檔案大小 (MB)</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>大尺寸的真實圖片檔案</returns>
        public static IFormFile CreateLargeImageFile(int sizeMB = 15, string fileName = "large_test_image.jpg")
        {
            // 計算所需的像素尺寸來達到目標檔案大小
            var targetPixels = sizeMB * 300; // 大約的像素數量
            var dimension = (int)Math.Sqrt(targetPixels);
            
            using var image = new Image<Rgb24>(dimension, dimension);
            
            // 填充複雜的圖案來增加檔案大小 - 使用像素操作
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    image[x, y] = new Rgb24(255, 255, 255); // 白色
                }
            }
            
            var stream = new MemoryStream();
            image.SaveAsJpeg(stream, new JpegEncoder { Quality = 100 }); // 最高品質增加檔案大小
            stream.Position = 0;
            
            return new FormFile(stream, 0, stream.Length, "LargePropertyImage", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        /// <summary>
        /// 建立無效格式的檔案 - 用於測試檔案格式驗證
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>無效格式的檔案</returns>
        public static IFormFile CreateInvalidFormatFile(string fileName = "invalid_file.txt")
        {
            var content = "這是一個純文字檔案，不是圖片或PDF檔案。\n用於測試檔案格式驗證功能。";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            
            return new FormFile(stream, 0, stream.Length, "InvalidFile", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }

        /// <summary>
        /// 建立多個真實圖片檔案
        /// </summary>
        /// <param name="count">圖片數量</param>
        /// <param name="baseFileName">基礎檔案名稱</param>
        /// <returns>真實圖片檔案列表</returns>
        public static List<IFormFile> CreateMultipleRealImages(int count = 3, string baseFileName = "property_image")
        {
            var files = new List<IFormFile>();
            
            for (int i = 1; i <= count; i++)
            {
                var fileName = $"{baseFileName}_{i:D2}.jpg";
                var width = 800 + (i * 100); // 不同大小的圖片
                var height = 600 + (i * 75);
                
                files.Add(CreateRealJpegImage(width, height, 85, fileName));
            }
            
            return files;
        }

        /// <summary>
        /// 建立真實的 IFormFileCollection - 解決 UploadImagesAsync 的型別要求
        /// </summary>
        /// <param name="count">圖片數量</param>
        /// <param name="baseFileName">基礎檔案名稱</param>
        /// <returns>真實的 IFormFileCollection</returns>
        public static IFormFileCollection CreateRealFormFileCollection(int count = 3, string baseFileName = "property_image")
        {
            var formFileCollection = new FormFileCollection();
            
            for (int i = 1; i <= count; i++)
            {
                var fileName = $"{baseFileName}_{i:D2}.jpg";
                var width = 800 + (i * 100); // 不同大小的圖片
                var height = 600 + (i * 75);
                
                var imageFile = CreateRealJpegImage(width, height, 85, fileName);
                formFileCollection.Add(imageFile);
            }
            
            return formFileCollection;
        }

    }
}