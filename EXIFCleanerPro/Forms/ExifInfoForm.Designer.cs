namespace EXIFCleanerPro.Forms
{
    partial class ExifInfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            btnExit = new Button();
            button1 = new Button();
            listViewExif = new ListView();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnExit);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 481);
            panel1.Name = "panel1";
            panel1.Size = new Size(477, 45);
            panel1.TabIndex = 0;
            // 
            // btnExit
            // 
            btnExit.DialogResult = DialogResult.Cancel;
            btnExit.Location = new Point(380, 11);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(75, 23);
            btnExit.TabIndex = 1;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(12, 11);
            button1.Name = "button1";
            button1.Size = new Size(116, 23);
            button1.TabIndex = 0;
            button1.Text = "Copy to Clipboard";
            button1.UseVisualStyleBackColor = true;
            // 
            // listViewExif
            // 
            listViewExif.Columns.AddRange(new ColumnHeader[] { columnHeader2, columnHeader3 });
            listViewExif.Dock = DockStyle.Fill;
            listViewExif.FullRowSelect = true;
            listViewExif.GridLines = true;
            listViewExif.Location = new Point(0, 0);
            listViewExif.Name = "listViewExif";
            listViewExif.Size = new Size(477, 481);
            listViewExif.TabIndex = 1;
            listViewExif.UseCompatibleStateImageBehavior = false;
            listViewExif.View = View.Details;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "EXIF Tag";
            columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Value";
            columnHeader3.Width = 300;
            // 
            // ExifInfoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(477, 526);
            Controls.Add(listViewExif);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "ExifInfoForm";
            Text = "ExifInfoForm";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnExit;
        private Button button1;
        private ListView listViewExif;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
    }
}