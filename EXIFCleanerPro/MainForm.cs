using EXIFCleanerPro.Forms;
using EXIFCleanerPro.Services;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace EXIFCleanerPro
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        // Import the User32.dll library to release mouse capture and send messages to the window
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        // Define constants for window dragging
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private static readonly HashSet<string> allowedImageExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

        public MainForm()
        {
            InitializeComponent();
            SetFormRoundedEdges();
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Release the mouse capture and send the WM_NCLBUTTONDOWN message to simulate window drag
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SetFormRoundedEdges()
        {
            GraphicsPath path = new GraphicsPath();
            int cornerRadius = 20;

            // Top-left corner
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            // Top-right corner
            path.AddArc(this.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            // Bottom-right corner
            path.AddArc(this.Width - cornerRadius, this.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            // Bottom-left corner
            path.AddArc(0, this.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);

            path.CloseAllFigures();
            this.Region = new Region(path);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Allow the selection of multiple files
                openFileDialog.Multiselect = true;

                // Set the file filter to image files
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|All Files|*.*";

                // Show the dialog and check if the user clicked OK
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected files
                    string[] selectedFiles = openFileDialog.FileNames;

                    // Add each selected file to your ListView or other UI
                    foreach (string file in selectedFiles)
                    {
                        // Get the file size in KB
                        long fileSizeInBytes = new FileInfo(file).Length;
                        long fileSizeInKB = fileSizeInBytes / 1024;  // Convert to KB

                        ListViewItem item = new ListViewItem(Path.GetFileName(file))
                        {
                            Checked = true
                        };
                        item.SubItems.Add(file);
                        item.SubItems.Add($"{fileSizeInKB} KB");
                        listViewImages.Items.Add(item);
                    }
                }
            }
        }

        private void buttonAddFolder_Click(object sender, EventArgs e)
        {
            // Show the folder browser dialog
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected folder path
                string selectedFolder = folderBrowserDialog.SelectedPath;

                // Optionally: Get all image files in the selected folder
                string[] imageFiles = Directory.GetFiles(selectedFolder, "*.*", SearchOption.TopDirectoryOnly)
                                              .Where(file => allowedImageExtensions.Contains(Path.GetExtension(file).ToLower()))
                                              .ToArray();

                // Add the folder or image files to your ListView
                foreach (string file in imageFiles)
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(file));
                    item.SubItems.Add(file);  // Optionally add full path as a sub-item
                    listViewImages.Items.Add(item);
                }
            }
        }

        private void ShowExifForm(string filePath)
        {
            ExifInfoForm exifForm = new ExifInfoForm();
            exifForm.LoadExifData(filePath);
            exifForm.ShowDialog();
        }

        private void listViewImages_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Check if an item is selected
            if (listViewImages.SelectedItems.Count > 0)
            {
                // Get the file path from the selected item
                string filePath = listViewImages.SelectedItems[0].SubItems[1].Text;

                // Show EXIF information in a new form
                ShowExifForm(filePath);
            }
        }

        private void listViewImages_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listViewImages.GetItemAt(e.X, e.Y);

            if (item != null)
            {
                // Define the bounds for the checkbox area
                Rectangle checkBoxBounds = new Rectangle(item.Bounds.Left, item.Bounds.Top, 16, item.Bounds.Height);

                // If the click is outside the checkbox area, prevent the checkbox toggle
                if (!checkBoxBounds.Contains(e.Location))
                {
                    item.Checked = !item.Checked;  // Revert the checkbox state
                }
            }
        }

    }
}
