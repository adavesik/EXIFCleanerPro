namespace EXIFCleanerPro
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            btnClose = new Button();
            listViewImages = new ListView();
            FileName = new ColumnHeader();
            FilePath = new ColumnHeader();
            Size = new ColumnHeader();
            EXIFPresent = new ColumnHeader();
            btnAdd = new Button();
            btnClear = new Button();
            btnStart = new Button();
            btnExit = new Button();
            folderBrowserDialog = new FolderBrowserDialog();
            buttonAddFolder = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.Gray;
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnClose);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(592, 40);
            panel1.TabIndex = 0;
            panel1.MouseDown += Form_MouseDown;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.exifcleaner_logo_32x32;
            pictureBox1.Location = new Point(12, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(32, 32);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            label1.ForeColor = Color.White;
            label1.Location = new Point(50, 9);
            label1.Name = "label1";
            label1.Size = new Size(120, 20);
            label1.TabIndex = 1;
            label1.Text = "EXIFCleaner Pro";
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.IndianRed;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(552, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(40, 40);
            btnClose.TabIndex = 0;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // listViewImages
            // 
            listViewImages.AllowDrop = true;
            listViewImages.CheckBoxes = true;
            listViewImages.Columns.AddRange(new ColumnHeader[] { FileName, FilePath, Size, EXIFPresent });
            listViewImages.FullRowSelect = true;
            listViewImages.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewImages.Location = new Point(12, 82);
            listViewImages.MultiSelect = false;
            listViewImages.Name = "listViewImages";
            listViewImages.Size = new Size(443, 249);
            listViewImages.TabIndex = 1;
            listViewImages.UseCompatibleStateImageBehavior = false;
            listViewImages.View = View.Details;
            // 
            // FileName
            // 
            FileName.Text = "File Name";
            FileName.Width = 100;
            // 
            // FilePath
            // 
            FilePath.Text = "File Path";
            FilePath.Width = 200;
            // 
            // Size
            // 
            Size.Text = "Size";
            // 
            // EXIFPresent
            // 
            EXIFPresent.Text = "EXIF Present";
            EXIFPresent.Width = 80;
            // 
            // btnAdd
            // 
            btnAdd.BackgroundImageLayout = ImageLayout.None;
            btnAdd.Image = Properties.Resources.ui_element_15768319;
            btnAdd.ImageAlign = ContentAlignment.MiddleRight;
            btnAdd.Location = new Point(475, 82);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(105, 30);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add Images";
            btnAdd.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnClear
            // 
            btnClear.Image = Properties.Resources.remove_9013644_1_;
            btnClear.Location = new Point(475, 187);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(105, 30);
            btnClear.TabIndex = 3;
            btnClear.Text = "Clear";
            btnClear.TextAlign = ContentAlignment.MiddleRight;
            btnClear.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.DeepSkyBlue;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.FlatAppearance.MouseOverBackColor = Color.Cyan;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.ForeColor = SystemColors.Desktop;
            btnStart.Image = Properties.Resources.play_257213;
            btnStart.Location = new Point(475, 234);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(105, 46);
            btnStart.TabIndex = 4;
            btnStart.Text = "Start";
            btnStart.TextAlign = ContentAlignment.MiddleRight;
            btnStart.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnStart.UseVisualStyleBackColor = false;
            // 
            // btnExit
            // 
            btnExit.Image = Properties.Resources.close_10767183;
            btnExit.Location = new Point(475, 301);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(105, 30);
            btnExit.TabIndex = 5;
            btnExit.Text = "Exit";
            btnExit.TextAlign = ContentAlignment.MiddleRight;
            btnExit.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // buttonAddFolder
            // 
            buttonAddFolder.BackgroundImageLayout = ImageLayout.None;
            buttonAddFolder.Image = Properties.Resources.new_folder;
            buttonAddFolder.ImageAlign = ContentAlignment.MiddleRight;
            buttonAddFolder.Location = new Point(475, 118);
            buttonAddFolder.Name = "buttonAddFolder";
            buttonAddFolder.Size = new Size(105, 30);
            buttonAddFolder.TabIndex = 6;
            buttonAddFolder.Text = "Add Folder";
            buttonAddFolder.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonAddFolder.UseVisualStyleBackColor = true;
            buttonAddFolder.Click += buttonAddFolder_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(592, 405);
            Controls.Add(buttonAddFolder);
            Controls.Add(btnExit);
            Controls.Add(btnStart);
            Controls.Add(btnClear);
            Controls.Add(btnAdd);
            Controls.Add(listViewImages);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "MainForm";
            MouseDown += Form_MouseDown;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnClose;
        private PictureBox pictureBox1;
        private Label label1;
        private ListView listViewImages;
        private ColumnHeader FileName;
        private ColumnHeader FilePath;
        private ColumnHeader Size;
        private ColumnHeader EXIFPresent;
        private Button btnAdd;
        private Button btnClear;
        private Button btnStart;
        private Button btnExit;
        private FolderBrowserDialog folderBrowserDialog;
        private Button buttonAddFolder;
    }
}
