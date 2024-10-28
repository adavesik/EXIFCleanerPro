using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFCleanerPro.Forms
{
    public partial class ExifInfoForm : Form
    {
        public ExifInfoForm()
        {
            InitializeComponent();
        }

        public void LoadExifData(string filePath)
        {
            // Clear any previous items
            listViewExif.Items.Clear();

            try
            {
                // Read metadata using MetadataExtractor
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                // Populate the ListView with tag-value pairs
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        ListViewItem item = new ListViewItem(tag.Name);  // EXIF Tag
                        item.SubItems.Add(tag.Description);               // EXIF Value
                        listViewExif.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading EXIF data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
