using Gif.Components;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Gifer
{
    public partial class Form1 : Form
    {
        public static Random rd = new Random();
        public string dir = "";
        public string tmp = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var files = openFileDialog1.FileNames;
                foreach (var s in files)
                    listBox1.Items.Add(s);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var index = listBox1.SelectedIndex - 1;
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                if (index >= 0) listBox1.SelectedIndex = index;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                label1.Text = "输出文件夹：" + folderBrowserDialog1.SelectedPath;
            }
        }

        public bool UnZip(string fileToUnZip, string zipedFolder, string password)
        {
            bool result = true;
            FileStream fs = null;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            string fileName;

            if (!File.Exists(fileToUnZip))
                return false;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            try
            {
                zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));
                if (!string.IsNullOrEmpty(password)) zipStream.Password = password;
                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!string.IsNullOrEmpty(ent.Name))
                    {
                        fileName = Path.Combine(zipedFolder, ent.Name);
                        fileName = fileName.Replace('/', '\\');//change by Mr.HopeGi   

                        if (fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }

                        fs = File.Create(fileName);
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                                fs.Write(data, 0, data.Length);
                            else
                                break;
                        }
                        if (fs != null)
                        {
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }
            finally
            {
                /*if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }*/
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }

        public bool UnZip(string fileToUnZip, string zipedFolder)
        {
            bool result = UnZip(fileToUnZip, zipedFolder, null);
            return result;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count <= 0)
            {
                MessageBox.Show("没有添加任何文件");
                return;
            }
            if (label1.Text.Substring(6).Length <= 0)
            {
                MessageBox.Show("输出路径为空");
                return;
            }
            foreach (var item in listBox1.Items)
            {
                dir = rdPath();
                try
                {
                    if(UnZip(item.ToString(), Path.GetTempPath() + dir))
                    {
                        try
                        {
                            tmp = item.ToString();
                            tmp = tmp.Substring(tmp.LastIndexOf("\\") + 1);
                            tmp = tmp.Substring(0, tmp.IndexOf('.') - 1) + ".gif";
                            ToGif(Path.GetTempPath() + dir, label1.Text.Substring(6) + "\\" + tmp, Int32.Parse(textBox1.Text), true);
                        }
                        catch
                        {
                            MessageBox.Show("转换失败,跳过该文件");
                            Directory.Delete(Path.GetTempPath() + dir, true);
                            continue;
                        }
                    }
                    else
                    {
                        MessageBox.Show("解压失败,跳过该文件");
                        Directory.Delete(Path.GetTempPath() + dir, true);
                        continue;
                    }
                }
                catch
                {
                    MessageBox.Show("解压失败,跳过该文件");
                    Directory.Delete(Path.GetTempPath() + dir, true);
                    continue;
                }
                Directory.Delete(Path.GetTempPath() + dir, true);
            }
            MessageBox.Show("转换结束");
        }

        public void ToGif(string directory, string giffile, int time, bool repeat)
        {
            string[] files = Directory.GetFileSystemEntries(directory, "*.jpg");

            AnimatedGifEncoder e = new AnimatedGifEncoder();
            e.Start(giffile);
            e.SetDelay(time);
            e.SetRepeat(repeat ? 0 : -1);
            for (int i = 0, count = files.Length; i < count; i++)
            {
                var img = Image.FromFile(files[i]);
                e.AddFrame(img);
                img.Dispose();
                progressBar1.Value = (int)(i * 1.0 / files.Length * 100);
                Application.DoEvents();
            }
            e.Finish();
            progressBar1.Value = 0;
        }

        public string rdPath()
        {
            string ret = "";
            for (int i = 0; i < 6; i++)
            {
                int tmp = rd.Next(36);
                if (tmp >= 10) ret += (char)((int)'a' + (tmp - 10));
                else ret += tmp;
            }
            if (Directory.Exists(Path.GetTempPath() + ret)) return rdPath();
            else return ret;
        }
    }
}
