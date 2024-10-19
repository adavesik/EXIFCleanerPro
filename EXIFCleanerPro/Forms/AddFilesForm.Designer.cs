namespace EXIFCleanerPro.Forms
{
    partial class AddFilesForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddFilesForm));
            listViewSelected = new ListView();
            buttonAdd = new Button();
            buttonRemove = new Button();
            label1 = new Label();
            label2 = new Label();
            buttonOK = new Button();
            buttonCancel = new Button();
            imageList1 = new ImageList(components);
            treeViewAvailable = new TreeView();
            SuspendLayout();
            // 
            // listViewSelected
            // 
            listViewSelected.Location = new Point(345, 24);
            listViewSelected.Name = "listViewSelected";
            listViewSelected.Size = new Size(227, 322);
            listViewSelected.TabIndex = 1;
            listViewSelected.UseCompatibleStateImageBehavior = false;
            // 
            // buttonAdd
            // 
            buttonAdd.BackColor = Color.LimeGreen;
            buttonAdd.FlatAppearance.BorderSize = 0;
            buttonAdd.FlatStyle = FlatStyle.Flat;
            buttonAdd.Location = new Point(254, 150);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(85, 23);
            buttonAdd.TabIndex = 2;
            buttonAdd.Text = "Add >>";
            buttonAdd.UseVisualStyleBackColor = false;
            buttonAdd.Click += button1_Click;
            // 
            // buttonRemove
            // 
            buttonRemove.BackColor = Color.IndianRed;
            buttonRemove.FlatAppearance.BorderSize = 0;
            buttonRemove.FlatStyle = FlatStyle.Flat;
            buttonRemove.Location = new Point(254, 197);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(85, 23);
            buttonRemove.TabIndex = 3;
            buttonRemove.Text = "Remove <<";
            buttonRemove.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 7F);
            label1.ForeColor = Color.White;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(215, 12);
            label1.TabIndex = 4;
            label1.Text = "Add images or folder to the list on the right side";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 7F);
            label2.ForeColor = Color.White;
            label2.Location = new Point(345, 9);
            label2.Name = "label2";
            label2.Size = new Size(19, 12);
            label2.TabIndex = 5;
            label2.Text = "List";
            // 
            // buttonOK
            // 
            buttonOK.BackColor = Color.LightSkyBlue;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.FlatAppearance.BorderSize = 0;
            buttonOK.FlatStyle = FlatStyle.Flat;
            buttonOK.Location = new Point(382, 352);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 6;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = false;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(482, 352);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "desktop.png");
            // 
            // treeViewAvailable
            // 
            treeViewAvailable.ImageIndex = 0;
            treeViewAvailable.ImageList = imageList1;
            treeViewAvailable.Location = new Point(12, 24);
            treeViewAvailable.Name = "treeViewAvailable";
            treeViewAvailable.SelectedImageIndex = 0;
            treeViewAvailable.Size = new Size(236, 322);
            treeViewAvailable.TabIndex = 8;
            // 
            // AddFilesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(584, 388);
            Controls.Add(treeViewAvailable);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(buttonRemove);
            Controls.Add(buttonAdd);
            Controls.Add(listViewSelected);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "AddFilesForm";
            Text = "Add Files/folders to...";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public ListView listViewSelected;
        private Button buttonAdd;
        private Button buttonRemove;
        private Label label1;
        private Label label2;
        private Button buttonOK;
        private Button buttonCancel;
        private ImageList imageList1;
        private TreeView treeViewAvailable;
    }
}