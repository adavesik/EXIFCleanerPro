using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
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

                using (var sourceImage = Image.FromFile(inputFilePath))
                {
                    ApplyExifOrientation(sourceImage);
                    PixelFormat pixelFormat = extension == ".png"
                        ? PixelFormat.Format32bppArgb
                        : PixelFormat.Format24bppRgb;
                    using Bitmap cleanBitmap = new(sourceImage.Width, sourceImage.Height, pixelFormat);
                    if (sourceImage.HorizontalResolution > 0 && sourceImage.VerticalResolution > 0)
                    {
                        cleanBitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    }

                    using (Graphics graphics = Graphics.FromImage(cleanBitmap))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(sourceImage, new Rectangle(0, 0, cleanBitmap.Width, cleanBitmap.Height));
                    }

                    if (extension == ".jpg" || extension == ".jpeg")
                    {
                        // For JPEG, save with high quality to strip EXIF
                        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        cleanBitmap.Save(outputFilePath, encoder, encoderParams);
                    }
                    else if (extension == ".png")
                    {
                        // For PNG, save as PNG (EXIF is not standard in PNG, but this strips any metadata)
                        cleanBitmap.Save(outputFilePath, ImageFormat.Png);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cleaning EXIF data: {ex.Message}", ex);
            }
        }

        private static void ApplyExifOrientation(Image image)
        {
            const int OrientationPropertyId = 0x0112;
            if (!image.PropertyIdList.Contains(OrientationPropertyId))
            {
                return;
            }

            PropertyItem? orientationProperty = image.GetPropertyItem(OrientationPropertyId);
            if (orientationProperty?.Value is null || orientationProperty.Value.Length < 2)
            {
                return;
            }

            ushort orientation = BitConverter.ToUInt16(orientationProperty.Value, 0);
            RotateFlipType transform = orientation switch
            {
                2 => RotateFlipType.RotateNoneFlipX,
                3 => RotateFlipType.Rotate180FlipNone,
                4 => RotateFlipType.Rotate180FlipX,
                5 => RotateFlipType.Rotate90FlipX,
                6 => RotateFlipType.Rotate90FlipNone,
                7 => RotateFlipType.Rotate270FlipX,
                8 => RotateFlipType.Rotate270FlipNone,
                _ => RotateFlipType.RotateNoneFlipNone
            };
            if (transform != RotateFlipType.RotateNoneFlipNone)
            {
                image.RotateFlip(transform);
            }
        }
    }
}
