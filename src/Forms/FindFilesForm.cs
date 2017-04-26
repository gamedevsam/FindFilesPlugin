using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PluginCore;
using System.Data;
using System.IO;
using Win32;
using System.Threading;

namespace FindFilesPlugin
{
    public partial class FindFilesForm : Form
    {
        private bool isInitialized = false;

        private static string SearchText = "";
        private SearchManager searchManager;

        private Font nameFont;
        private Font pathFont;
        private PluginMain plugin;
        private Settings pluginSettings;
        private HiPerfTimer timer;

        private String directoryInfoText;

        public  static List<String> cashedFiles = new List<String>();
        private static volatile EventHandler itemsLoadedDelegate;
        private static volatile bool filesLoading;
        private static Thread fileLoaderThread;

        public FindFilesForm(PluginMain plugin)
        {
            this.plugin = plugin;
            pluginSettings = plugin.Settings as Settings;
            InitializeComponent();

            this.toolTipReload.SetToolTip(this.btnReload, "\nThe FindFiles plugin caches the file list the first time it opens to speed up search time.\nThis button causes the plugin to relaunch and refresh the search results.\nUse this if you added or removed files in the search directories since you first started FindFiles.");
            this.toolTipInProjectOnly.SetToolTip(this.checkInProjectOnly, "\nOnly files in the project directory will be searched. Changing this requires the file cache to be reset.");

            timer = new HiPerfTimer();

            // Restore saved settings
            if (pluginSettings.FindFilesFormSize.Width > MinimumSize.Width)
                Size = pluginSettings.FindFilesFormSize;
            if (pluginSettings.FileNameWidth > 0)
                dataGridView.Columns[0].Width = pluginSettings.FileNameWidth;
            if (pluginSettings.FilePathWidth > 0)
                dataGridView.Columns[1].Width = pluginSettings.FilePathWidth;
            
            if (PluginBase.CurrentProject == null)
                checkInProjectOnly.Enabled = false;
            else
                checkInProjectOnly.Checked = pluginSettings.SearchInProjectOnly;

            Text = "Find Files   " + pluginSettings.SearchFilter;

            nameFont = new Font("Courier New", 10, FontStyle.Bold);
            pathFont = new Font("Courier New", 8, FontStyle.Italic);

            dataGridView.Columns[0].DefaultCellStyle.Font = nameFont;
            dataGridView.Columns[1].DefaultCellStyle.Font = pathFont;

            directoryInfoText = String.Empty;
            if (pluginSettings.SearchDirectory != pluginSettings.DefaultEmptyString)
                if (Directory.Exists(pluginSettings.SearchDirectory))
                    directoryInfoText = pluginSettings.SearchDirectory;
            if ((pluginSettings.SearchProject || pluginSettings.SearchInProjectOnly) && PluginBase.CurrentProject != null)
                if (directoryInfoText != String.Empty)
                    directoryInfoText = "Project Folder" + "  &&  " + directoryInfoText;
                else
                    directoryInfoText = "Project Folder";

            labelSearchTime.Text = "Search Time: ...";
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CreateFileList();
            textBox.Focus();
            textBox.Text = SearchText;
            textBox.Select(0, SearchText.Length);
            isInitialized = true;
        }

        #region FILE LOADING THREADING
        private void CreateFileList()
        {
            searchManager = new SearchManager(plugin, dataGridView);

            if (cashedFiles.Count == 0)
            {
                btnReload.Enabled = false;
                checkInProjectOnly.Enabled = false;
                labelDirectoryInfo.Text = "Calculating File Count...";
                labelSearchTime.Text = "";

                string updatingCacheText = "Updating file cache, please wait...";
                dataGridView.Rows.Add(updatingCacheText);

                // Resize name column to fit the text
                Size s = new Size();
                Font font = dataGridView.Columns[0].DefaultCellStyle.Font;
                s = TextRenderer.MeasureText(updatingCacheText, font);
                dataGridView.Columns[0].Width = s.Width + 10 /* some padding */;
                dataGridView.ClearSelection();
                dataGridView.Enabled = false;

                filesLoading = true;
                itemsLoadedDelegate = new EventHandler(FilesLoadedCallback);
                fileLoaderThread = new Thread(new ThreadStart(LoadFilesThread));
                fileLoaderThread.Start();
            }
            else
            {
                filesLoading = false;

                foreach (string file in cashedFiles)
                {
                    searchManager.AddFileToSearchList(file);
                }

                if (SearchText != "")
                {
                    textBox.Text = SearchText;
                    textBox.Select(0, SearchText.Length);
                }

                labelDirectoryInfo.Text = directoryInfoText + "     Files: " + searchManager.fileList.Count;
            }
        }

        private void LoadFilesThread()
        {
            try
            {
                cashedFiles = plugin.GetFiles(checkInProjectOnly.Checked);
                // Remove any files that are in the excluded directories
                string[] excludedDirectories = plugin.settingObject.ExcludedDirectories.Split(new char[] { ',', ';' });
                for (int i = 0; i < cashedFiles.Count; )
                {
                    bool fileRemoved = false;

                    if (excludedDirectories.Length > 0 && excludedDirectories[0] != plugin.settingObject.DefaultEmptyString)
                    {
                        foreach (string excludedDir in excludedDirectories)
                        {
                            if (cashedFiles[i].StartsWith(excludedDir))
                            {
                                fileRemoved = true;
                                break;
                            }
                        }
                    }

                    if (!fileRemoved)
                    {
                        searchManager.AddFileToSearchList(cashedFiles[i]);
                        i++;
                    }
                    else
                        cashedFiles.RemoveAt(i);
                }

                if (FindFilesForm.itemsLoadedDelegate != null)
                    Invoke(FindFilesForm.itemsLoadedDelegate);
            }
            catch (ThreadAbortException)
            {
                // Do nothing, we're good
                PluginMain.ClearCachedFiles();
                FindFilesForm.itemsLoadedDelegate = null;
            }
        }

        private void FilesLoadedCallback(object sender, EventArgs e)
        {
            btnReload.Enabled = true;

            if (PluginBase.CurrentProject == null)
            {
                checkInProjectOnly.Enabled = false;
            }
            else
            {
                checkInProjectOnly.Enabled = true;
                checkInProjectOnly.Checked = pluginSettings.SearchInProjectOnly;
            }

            labelDirectoryInfo.Text = directoryInfoText + "     Files: " + searchManager.fileList.Count;
            labelSearchTime.Text = "Search Time: ...";

            dataGridView.Enabled = true;
            RefreshDataGrid();
            filesLoading = false;
        }
        #endregion

        private void RefreshDataGrid()
        {
          dataGridView.Rows.Clear();
            if (textBox.Text.Length > 0)
            {
                timer.Start();
                searchManager.DoSearch(textBox.Text);
                timer.Stop();
                labelSearchTime.Text = "Search Time: " + String.Format("{0:0.###}", timer.Duration);

                if (timer.Duration < 0.25)
                    labelSearchTime.ForeColor = Color.Green;
                else if (timer.Duration >= 0.25 && timer.Duration < 0.5)
                    labelSearchTime.ForeColor = Color.Blue;
                else if (timer.Duration >= 0.5 && timer.Duration < 0.8)
                    labelSearchTime.ForeColor = Color.SaddleBrown;
                else
                    labelSearchTime.ForeColor = Color.Red;
            }
            else
            {
                labelSearchTime.Text = "Search Time: ...";
                labelSearchTime.ForeColor = Color.Black;
            }
            if (dataGridView.Rows.Count > 0)
                dataGridView.CurrentCell = dataGridView[0, 0];
        }

        private void Navigate()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                string path = (string)dataGridView.SelectedRows[0].Cells[1].Value;
                // Get Full File Path
                if (pluginSettings.SearchDirectory != pluginSettings.DefaultEmptyString)
                {
                    if (File.Exists(pluginSettings.SearchDirectory + path))
                        path = pluginSettings.SearchDirectory + path;
                }
                if (PluginBase.CurrentProject != null)
                {
                    String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
                    if (File.Exists(projectFolder + path))
                        path = projectFolder + path;
                }
                PluginBase.MainForm.OpenEditableDocument(path);
                Close();
            }
        }

        private void OpenResourceForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                Navigate();
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                dataGridView.Focus();
                SendKeys.Send("{DOWN}");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                dataGridView.Focus();
                SendKeys.Send("{UP}");
                e.Handled = true;
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (!filesLoading && (SearchText != textBox.Text || dataGridView.RowCount == 0))
                RefreshDataGrid();

            SearchText = textBox.Text;
        }

        private void OpenResourceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FindFilesForm.itemsLoadedDelegate = null;
            if (fileLoaderThread != null && fileLoaderThread.IsAlive)
                fileLoaderThread.Abort();

            pluginSettings.FindFilesFormSize = Size;
            pluginSettings.FileNameWidth = dataGridView.Columns[0].Width;
            pluginSettings.FilePathWidth = dataGridView.Columns[1].Width;
        }

        private void dataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Navigate();
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (dataGridView.RowCount == 0)
            {
                textBox.Focus();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Down && dataGridView.CurrentRow.Index < dataGridView.Rows.Count - 1)
            {
                dataGridView.ClearSelection();
                dataGridView.CurrentCell = dataGridView[0, dataGridView.CurrentRow.Index + 1];
                dataGridView.Focus();
            }
            else if (e.KeyCode == Keys.Up && dataGridView.CurrentRow.Index > 0)
            {
                dataGridView.ClearSelection();
                dataGridView.CurrentCell = dataGridView[0, dataGridView.CurrentRow.Index - 1];
                dataGridView.Focus();
            }
            else if (e.KeyCode == Keys.Up && dataGridView.CurrentRow.Index <= 0)
                textBox.Focus();

            e.Handled = true;
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Clear();
            PluginMain.ClearCachedFiles();
            CreateFileList();
        }

        private void checkInProjectOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (isInitialized)
            {
                pluginSettings.SearchInProjectOnly = checkInProjectOnly.Checked;
                btnReload_Click(sender, e);
            }
        }
    }
}