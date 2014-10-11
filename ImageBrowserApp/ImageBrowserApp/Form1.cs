using ImageBrowserApp.ImageBrowserDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageBrowserApp
{
    public partial class Form1 : Form
    {
        private string dirName = "";
        private string[] files;
        //List<System.IO.FileInfo> images;
        ImportProgress import_progress;
        DateTime selected_gallery;

        public Form1()
        {
            InitializeComponent();

            loadGalleriesList();
        }

        private void loadGalleriesList()
        {
            DataClasses1DataContext data = new DataClasses1DataContext();

            var galleries = from i in data.Images
                            let date = (DateTime)i.date_time
                            group i by new
                            {
                                date = date.Date
                            } into gallery
                            select new
                            {
                                Count = gallery.Count(),
                                Date = gallery.Key.date
                            };

            TreeNode parent = treeView1.Nodes[0];
            parent.Nodes.Clear();
            foreach (var img in galleries)
            {
                //MessageBox.Show(img.Count.ToString());
                TreeNode node = new TreeNode();
                node.Text =img.Date.ToString("dd.MM.yyyy") + " (" + img.Count + ")";
                node.Tag = img.Date;
                parent.Nodes.Add(node);
            }
            treeView1.Refresh();
        }

        private void galleryItem_Click(object sender, EventArgs e)
        {
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                dirName = folderBrowserDialog1.SelectedPath;
                if (dirName != "")
                {
                    try
                    {
                        files = System.IO.Directory.GetFiles(dirName, "*.jpg", System.IO.SearchOption.AllDirectories);
                    }
                    catch
                    {
                    }
                    //spr ile; > 0 to co nizej, mniej komunikat ze nie ma zdec
                                        
                    //pasek postepu bo IO moze zarzynac w ciul
                    if (files != null && backgroundWorker1.IsBusy != true)
                    {
                        // create a new instance of the alert form
                        import_progress = new ImportProgress();
                        import_progress.Show();
                        // Start the asynchronous operation.
                        backgroundWorker1.RunWorkerAsync();
                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            System.IO.FileInfo fi = null;
            //images = new List<System.IO.FileInfo>();

            int i = 1;
            int count = files.Length;// -1;

            foreach (string f in files)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    i = 1;
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(50);//do wywalenia ofc
                    worker.ReportProgress(i * 100/count);
                    i++;

                    try
                    {
                        fi = new System.IO.FileInfo(f);
                        //images.Add(fi);

                        try
                        {
                            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImageBrowserApp.Properties.Settings.ImageBrowserConnectionString"].ConnectionString))
                            {
                                conn.Open();
                                using (SqlCommand cmd =
                                    new SqlCommand("INSERT INTO Images VALUES(" +
                                        "@path, @name, @date_time)", conn))
                                {
                                    //cmd.Parameters.AddWithValue("@id", null);
                                    cmd.Parameters.AddWithValue("@path", fi.FullName);
                                    cmd.Parameters.AddWithValue("@name", fi.Name);
                                    cmd.Parameters.AddWithValue("@date_time", fi.CreationTime);

                                    int rows = cmd.ExecuteNonQuery();

                                    //rows number of record got inserted
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            //MessageBox.Show(ex.ToString());
                            //Log exception
                            //Display Error message
                        }
                    }
                    catch
                    {
                        //tylko logowac
                    }
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            import_progress.Message = "Import in progress, please wait... " + e.ProgressPercentage.ToString() + "%";
            import_progress.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //textBox1.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                //textBox1.Text = "Error: " + e.Error.Message;
            }
            else
            {
                //textBox1.Text = "Done!";
            }
            // Close the AlertForm
            import_progress.Close();
            loadGalleriesList();

            //string tmp = "";
            /*foreach(var img in images)
            {
                //tmp += img.FullName.ToString() + "\r\n";

                PictureBox item = new PictureBox();
                item.Height = 100;
                item.ImageLocation = img.FullName;
                item.SizeMode = PictureBoxSizeMode.StretchImage;


                galleryPanel.Controls.Add(item);
            }*/
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level > 0)
            {
                selected_gallery = ((DateTime)e.Node.Tag).Date;
                reloadSelectedGallery();// (selected_gallery);
            }
        }

        private void reloadSelectedGallery()//(DateTime e)
        {
            //MessageBox.Show(e.ToString());

            DataClasses1DataContext data = new DataClasses1DataContext();
            IEnumerable<Image> images = from i in data.Images
                                        where i.date_time.Value.Date == selected_gallery
                                        select i;

            galleryPanel.Controls.Clear();

            foreach (var img in images)
            {
                PictureBox item = new PictureBox();
                item.Height = 100;
                item.ImageLocation = img.path;
                item.SizeMode = PictureBoxSizeMode.StretchImage;
                item.Click += new System.EventHandler(galleryItem_Click);

                ContextMenu mnu = new ContextMenu();
                MenuItem mnuRemove = new MenuItem("Remove");
                mnuRemove.Click += new EventHandler(mnuRemove_Click);
                mnuRemove.Tag = img.id;
                mnu.MenuItems.Add(mnuRemove);
                item.ContextMenu = mnu;

                galleryPanel.Controls.Add(item);
            }
        }

        private void mnuRemove_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ImageBrowserApp.Properties.Settings.ImageBrowserConnectionString"].ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd =
                        new SqlCommand("DELETE Images WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", ((MenuItem)sender).Tag.ToString());
                        cmd.ExecuteNonQuery();
                        loadGalleriesList();
                        reloadSelectedGallery();
                    }
                }
            }
            catch (SqlException ex)
            {
                //MessageBox.Show(ex.ToString());
                //Log exception
                //Display Error message
            }
            //MessageBox.Show(((MenuItem)sender).Tag.ToString());
        }
    }
}
