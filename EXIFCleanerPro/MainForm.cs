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
    }
}
