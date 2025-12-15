using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Imaging;

namespace EXIFCleanerPro.Services
{
    internal class ExifHelper
    {
        // Method to extract and format EXIF data
        public static string GetExifData(string filePath)
        {
            try
            {
                // Validate if file exists
                if (!File.Exists(filePath))
                    return "File not found.";

                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // Build a formatted string with EXIF data
                StringBuilder exifData = new StringBuilder();
                exifData.AppendLine("EXIF Information:");
                exifData.AppendLine(new string('-', 20));  // Separator line

                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        exifData.AppendLine($"{tag.Name}: {tag.Description}");
                    }
                }

                return exifData.ToString();
            }
            catch (Exception ex)
            {
                return $"Error reading EXIF data: {ex.Message}";
            }
        }

        // Method to clean EXIF data from an image file
        public static bool CleanExifData(string inputFilePath, string outputFilePath)
        {
            try
            {
                if (!File.Exists(inputFilePath))
                    throw new FileNotFoundException("Input file not found.", inputFilePath);

                string extension = Path.GetExtension(inputFilePath).ToLower();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    throw new NotSupportedException("Only JPG and PNG files are supported for cleaning.");

                using (var image = Image.FromFile(inputFilePath))
                {
                    if (extension == ".jpg" || extension == ".jpeg")
                    {
                        // For JPEG, save with high quality to strip EXIF
                        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        image.Save(outputFilePath, encoder, encoderParams);
                    }
                    else if (extension == ".png")
                    {
                        // For PNG, save as PNG (EXIF is not standard in PNG, but this strips any metadata)
                        image.Save(outputFilePath, ImageFormat.Png);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cleaning EXIF data: {ex.Message}", ex);
            }
        }
    }
}
