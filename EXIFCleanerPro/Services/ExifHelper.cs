using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
