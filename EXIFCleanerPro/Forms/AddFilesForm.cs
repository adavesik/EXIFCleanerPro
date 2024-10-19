using EXIFCleanerPro.Services;
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
    public partial class AddFilesForm : Form
    {
        public AddFilesForm()
        {
            InitializeComponent();
            PopulateTreeView();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)); // Start at Desktop
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name)
                {
                    Tag = info,
                    ImageIndex = AddIconToImageList(info.FullName, true) // Add folder icon
                };
                GetDirectories(info.GetDirectories(), rootNode);
                treeViewAvailable.Nodes.Add(rootNode);
            }
        }

        // Recursively populate directories and files
        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, AddIconToImageList(subDir.FullName, true), AddIconToImageList(subDir.FullName, true));
                aNode.Tag = subDir;
                aNode.ImageIndex = AddIconToImageList(subDir.FullName, true); // Folder icon
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }

            // Add files in the folder
            FileInfo[] files = ((DirectoryInfo)nodeToAddTo.Tag).GetFiles();
            foreach (FileInfo file in files)
            {
                TreeNode fileNode = new TreeNode(file.Name, AddIconToImageList(file.FullName, false), AddIconToImageList(file.FullName, false));
                fileNode.Tag = file;
                nodeToAddTo.Nodes.Add(fileNode);
            }
        }

        // Method to add system icons to the ImageList
        private int AddIconToImageList(string path, bool isFolder)
        {
            Icon systemIcon = SystemIconsHelper.GetSystemIcon(path, isFolder);

            // If no valid icon is returned, skip adding an icon or use a default icon
            if (systemIcon != null)
            {
                imageList1.Images.Add(systemIcon);  // Add the valid icon to the ImageList
                return imageList1.Images.Count - 1;  // Return the index of the newly added icon
            }
            else
            {
                // Return a default index or handle the case where no icon is available
                return -1;  // Indicate no icon was added
            }
        }

    }
}
