using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SimMetricsMetricUtilities;
using PluginCore;
using System.Drawing;

namespace FindFilesPlugin
{
    class SearchManager
    {
        // Information regarding the rest of the plugin.
        private DataGridView dataGridPanel;
        private PluginMain plugin;
        private Settings pluginSettings;
        // Keep track of added files to eliminate dupes in file list display.
        private Dictionary<String, object> eliminateDupes;

        // A list so that we can properly handle files with the same name.
        public List<FileData> fileList = new List<FileData>();

        #region SimMetrics Classes

        SmithWaterman smithWaterman = new SmithWaterman();
        SmithWatermanGotoh smithWatermanGotoh = new SmithWatermanGotoh();     
        Jaro jaro = new Jaro();
        JaroWinkler jaroWinkler = new JaroWinkler();
        #endregion

        public SearchManager(PluginMain plugin, DataGridView panel)
        {
            this.plugin = plugin;
            pluginSettings = plugin.Settings as Settings;
            dataGridPanel = panel;
        }

        public void AddFileToSearchList(string file)
        {
            int slashIndex = file.LastIndexOf('\\');
            String name = file.Substring(slashIndex + 1);
            fileList.Add(new FileData(name, file));
        }

        private void AddFileToDataGrid(FileData file)
        {
            // Fast and simple hack to keep the display list clear of dupes 
            // (caused by searching for files in a project that is contained within one of the Search Directories).
            if (!eliminateDupes.ContainsKey(file.Path))
                eliminateDupes.Add(file.Path, null);
            else
                return;

            string filePath           = file.Path;
            bool   fileFoundInProject = false;

            // Fix Paths so they only show relevant information
            if (PluginBase.CurrentProject != null)
            {
                String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
                if (filePath.Contains(projectFolder))
                {
                    filePath = filePath.Substring(projectFolder.Length);
                    fileFoundInProject = true;
                }
            }
            if ((pluginSettings.SearchDirectory != pluginSettings.DefaultEmptyString) && !fileFoundInProject)
            {
                if (filePath.Contains(pluginSettings.SearchDirectory))
                    filePath = filePath.Substring(pluginSettings.SearchDirectory.Length);
            }

            dataGridPanel.Rows.Add(file.Name, filePath);
        }

        public void DoSearch(string searchText)
        {
            List<FileData> tmpList = new List<FileData>();

            foreach (FileData file in fileList)
            {
                string fileName = pluginSettings.IgnoreCase ? file.Name.ToLower() : file.Name;
                file.Similarity = GetStringSimilarity(searchText, fileName);

                if (pluginSettings.NormalizeSearchResults)
                {
                    if (file.Similarity >= pluginSettings.NormalizedSearchThreshold)
                    {
                        file.Similarity = AdjustSimilarityBasedOnSearchPattern(file);
                        tmpList.Add(file);
                    }
                }
                else
                {
                    if (file.Similarity > 0)
                    {
                        file.Similarity = AdjustSimilarityBasedOnSearchPattern(file);
                        tmpList.Add(file);
                    }
                }
            }

            tmpList.Sort(delegate(FileData f1, FileData f2) { return f2.Similarity.CompareTo(f1.Similarity); });

            // Add files to the Data Grid
            eliminateDupes = new Dictionary<string,object>();
            int j = 0, maxFontWidth = 0;
            Size s = new Size();
            Font font = dataGridPanel.Columns[0].DefaultCellStyle.Font;
            foreach (FileData file in tmpList)
            {
                s = TextRenderer.MeasureText(file.Name, font);
                if (s.Width > maxFontWidth)
                    maxFontWidth = s.Width;

                AddFileToDataGrid(file);
                if (++j > pluginSettings.MaxResultsShown)
                    break;
            }

            // Resize name column to the biggest element on the list
            dataGridPanel.Columns[0].Width = maxFontWidth + 10 /* some padding */;
        }

        private double AdjustSimilarityBasedOnSearchPattern(FileData file)
        {
            if (pluginSettings.PrioritizedPattern != "" && pluginSettings.PatternMatchSimilarityBoost > 0)
            {
                string[] patterns = pluginSettings.PrioritizedPattern.Split(new char[] { ',', ';' });
                foreach (string pat in patterns)
                {
                    if (pluginSettings.ApplyPrioritizedPatternToFilepath ? file.Path.Contains(pat) : file.Name.Contains(pat) )
                        file.Similarity += pluginSettings.PatternMatchSimilarityBoost;
                }
            }

            if (PluginBase.CurrentProject != null && pluginSettings.InProjectDirectorySimilarityBoost > 0)
            {
                String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
                if (file.Path.StartsWith(@"\") || file.Path.StartsWith(projectFolder))
                    file.Similarity += pluginSettings.InProjectDirectorySimilarityBoost;
            }
            return file.Similarity;
        }

        private double GetStringSimilarity(string first, string second)
        {
            double similarity = 0.0;

            switch (pluginSettings.StringMatchingAlgorithm)
            {
                case StringMatchingAlgorithms.SmithWaterman:
                    if (pluginSettings.NormalizeSearchResults)
                        similarity = smithWaterman.GetSimilarity(first, second);
                    else
                        similarity = smithWaterman.GetUnnormalisedSimilarity(first, second);
                    break;
                case StringMatchingAlgorithms.SmithWatermanGotoh:
                    if (pluginSettings.NormalizeSearchResults)
                        similarity = smithWatermanGotoh.GetSimilarity(first, second);
                    else
                        similarity = smithWatermanGotoh.GetUnnormalisedSimilarity(first, second);
                    break;
                case StringMatchingAlgorithms.Jaro:
                    if (pluginSettings.NormalizeSearchResults)
                        similarity = jaro.GetSimilarity(first, second);
                    else
                        similarity = jaro.GetUnnormalisedSimilarity(first, second);
                    break;
                case StringMatchingAlgorithms.JaroWinkler:
                    if (pluginSettings.NormalizeSearchResults)
                        similarity = jaroWinkler.GetSimilarity(first, second);
                    else
                        similarity = jaroWinkler.GetUnnormalisedSimilarity(first, second);
                    break;
            }

            return similarity;
        }
    }

    class FileData
    {
        public FileData(String name, String path)
        {
            Name = name;
            Path = path;
            Similarity = 0.0;
        }
        public String Name;
        public String Path;
        public double Similarity;
    }
}
