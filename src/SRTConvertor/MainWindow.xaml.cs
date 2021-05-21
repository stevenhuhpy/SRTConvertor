using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SRTConvertor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Title = CommonDef.Title;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            DialogResult result = browseDialog.ShowDialog();

            if (!string.IsNullOrEmpty(browseDialog.SelectedPath))
            {
                tbSourceFolder.Text = browseDialog.SelectedPath;
            }

            tbDestFolder.Text = tbSourceFolder.Text;

            if (string.IsNullOrEmpty(tbSuffix.Text))
            {
                tbSuffix.Text = "convert";
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            tbOutputs.Text = "";

            AddOutputs("#############################", OutputLevel.None);
            AddOutputs("    Start to convert files    ", OutputLevel.None);
            AddOutputs("#############################", OutputLevel.None);

            var sourceFolder = tbSourceFolder.Text;
            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                AddOutputs("Invalid folder path!", OutputLevel.Error);
                return;
            }

            AddOutputs("Get source folder: " + tbSourceFolder.Text, OutputLevel.Info);

            DirectoryInfo di = new DirectoryInfo(sourceFolder);
            var files = di.GetFiles("*.srt", SearchOption.AllDirectories);
            var excludeFiles = di.GetFiles("*."+ tbSuffix.Text + ".srt", SearchOption.AllDirectories);

            AddOutputs(string.Format("Get {0} original srt files.", files.Length - excludeFiles.Length), OutputLevel.Info);

            foreach (var f in files)
            {
                if(!f.Name.Contains(tbSuffix.Text + ".srt"))
                {
                    ConvertFile(f.FullName, tbSuffix.Text);
                }
            }
        }

        /// <summary>
        /// Example:
        /// ------
        /// ignore: 1
        /// ignore: 00:00:00,000 --> 00:00:09,248
        ///         Docker and Kubernetes.Everyone's talking
        /// ignore: 
        /// ignore: 2
        /// ignore: 00:00:09,248 --> 00:00:11,290
        ///         about them, tons of people are using them,
        /// ignore: 
        /// ignore: 3
        /// ignore: 00:00:11,290 --> 00:00:14,956
        ///         and it is high time we all got our heads
        /// ---
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="suffix"></param>
        private void ConvertFile(string filename, string suffix)
        {
            String line;
            String convertedText = string.Empty;
            String destFile = string.Empty;

            try
            {
                //read contents from srt
                StreamReader sr = new StreamReader(filename);

                //ignore the first two lines of text
                line = sr.ReadLine();
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    line = sr.ReadLine();

                    if (!string.IsNullOrEmpty(convertedText) && !line.StartsWith(".") && !line.StartsWith("!") && !line.StartsWith(",") && !line.StartsWith("?"))
                    {
                        line = " " + line;
                    }
                    convertedText += line;

                    //ignore next three lines of text
                    line = sr.ReadLine();
                    line = sr.ReadLine();
                    line = sr.ReadLine();
                }

                sr.Close();

                destFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), System.IO.Path.GetFileNameWithoutExtension(filename) +
                    "." + suffix + System.IO.Path.GetExtension(filename));
                if (File.Exists(destFile))
                {
                    try
                    {
                        File.Delete(destFile);
                    }
                    catch (Exception err)
                    {
                        AddOutputs(err.Message, OutputLevel.Error);
                    }
                }

                FileStream fs = File.Create(destFile);

                StreamWriter srNew = new StreamWriter(fs);
                srNew.Write(convertedText);
                srNew.Close();
            }
            catch (Exception e)
            {
                AddOutputs(e.Message, OutputLevel.Error);
            }
            finally
            {
                AddOutputs("Generate file: " + destFile, OutputLevel.Success);
            }

        }

        private void AddOutputs(string line, OutputLevel level)
        {
            if (!string.IsNullOrEmpty(tbOutputs.Text))
            {
                tbOutputs.Text += "\r\n";
            }

            if (level == OutputLevel.None)
            {
                tbOutputs.Text = tbOutputs.Text + line;
            }
            else
            {
                tbOutputs.Text = tbOutputs.Text + " [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] [" + level.ToString() + "] " + line;
            }
        }

        private void btnCleanup_Click(object sender, RoutedEventArgs e)
        {
            tbOutputs.Text = "";

            AddOutputs("#############################", OutputLevel.None);
            AddOutputs("    Start to clean up folder    ", OutputLevel.None);
            AddOutputs("#############################", OutputLevel.None);

            var sourceFolder = tbSourceFolder.Text;
            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                AddOutputs("Invalid folder path!", OutputLevel.Error);
                return;
            }

            DirectoryInfo di = new DirectoryInfo(sourceFolder);
            var files = di.GetFiles("*.convert.srt", SearchOption.AllDirectories);

            AddOutputs(string.Format("Get {0} files to clean up.", files.Length), OutputLevel.Info);

            foreach (var f in files)
            {
                try
                {
                    File.Delete(f.FullName);
                    AddOutputs("file removed: " + f.FullName, OutputLevel.Success);
                }
                catch(Exception err)
                {
                    AddOutputs(err.Message, OutputLevel.Error);
                }
            }
        }
    }

    internal enum OutputLevel
    {
        None,
        Info,
        Debug,
        Warning,
        Error,
        Success
    }

}
