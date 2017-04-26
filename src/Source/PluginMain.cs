using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;

namespace FindFilesPlugin
{
    public class PluginMain : IPlugin
    {
        private const String PLUGIN_NAME = "FindFiles";
        private const String PLUGIN_GUID = "ac04a177-f578-47d7-87f1-0cbc0f834446";
        private const String PLUGIN_HELP = "www.flashdevelop.org/community/";
        private const String PLUGIN_AUTH = "Canab & Sam Batista";
        private const String SETTINGS_FILE = "Settings.fdb";
        private const String PLUGIN_DESC = "FindFiles Plugin";

        private String settingFilename;
        public  Settings settingObject;

        public static List<String> cachedFiles = new List<string>();

        private ControlClickManager controlClickManager;

        private ToolStripMenuItem findFilesMenuItem = null;
        private ToolStripMenuItem quickOutlineMenuItem = null;

        #region Required Properties

        /// <summary>
        /// For FD4 Compatibility
        /// </summary> 
        public Int32 Api
        {
            get { return 1; }
        }
        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public String Name
        {
            get { return PLUGIN_NAME; }
        }

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
        {
            get { return PLUGIN_GUID; }
        }

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
        {
            get { return PLUGIN_AUTH; }
        }

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
        {
            get { return PLUGIN_DESC; }
        }

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
        {
            get { return PLUGIN_HELP; }
        }

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public Object Settings
        {
            get { return settingObject; }
        }
        
        #endregion
        
        #region Required Methods
        
        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            InitBasics();
            LoadSettings();
            AddEventHandlers();
            CreateMenuItems();

            if (settingObject.CtrlClickEnabled)
                controlClickManager = new ControlClickManager();
        }
        
        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose()
        {
            SaveSettings();
        }

        public static void ClearCachedFiles()
        {
            PluginMain.cachedFiles = new List<string>();
            FindFilesForm.cashedFiles = new List<String>();
        }
        
        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
        {
            if (e.Type == EventType.FileSwitch)
            {
                if (controlClickManager != null && PluginBase.MainForm.CurrentDocument != null)
                    controlClickManager.SciControl = PluginBase.MainForm.CurrentDocument.SciControl;                
            }
        }
        
        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, PLUGIN_NAME);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);
            settingFilename = Path.Combine(dataPath, SETTINGS_FILE);
        }

        public void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.FileSwitch);
        }

        public void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("SearchMenu");

            findFilesMenuItem = new ToolStripMenuItem("Find Files",
                PluginBase.MainForm.FindImage("209"),
                new EventHandler(ShowResourceForm),
                settingObject.OpenResourceShortcut);

            quickOutlineMenuItem = new ToolStripMenuItem("Quick Outline",
            PluginBase.MainForm.FindImage("315|16|0|0"),
            new EventHandler(ShowOutlineForm),
            settingObject.QuickOutlineShortcut);

            PluginBase.MainForm.RegisterShortcutItem("SearchMenu.FindFiles", findFilesMenuItem);
            PluginBase.MainForm.RegisterShortcutItem("SearchMenu.QuickOutline", quickOutlineMenuItem);

            menu.DropDownItems.Insert(3, quickOutlineMenuItem);
            menu.DropDownItems.Insert(7, findFilesMenuItem);
        }        

        public void ShowResourceForm(object sender, EventArgs e)
        {
            new FindFilesForm(this).ShowDialog();
        }

        private void ShowOutlineForm(object sender, EventArgs e)
        {
            new QuickOutlineForm(this).ShowDialog();
        }
        
        public void LoadSettings()
        {
            if (File.Exists(settingFilename))
            {
                try
                {
                    settingObject = new Settings();
                    settingObject = (Settings) ObjectSerializer.Deserialize(settingFilename, settingObject);
                }
                catch
                {
                    settingObject = new Settings();
                    SaveSettings();
                }
            }
            else
            {
                settingObject = new Settings();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            ObjectSerializer.Serialize(settingFilename, settingObject);
        }

        public List<String> GetProjectDirectory()
        {
            List<String> folders = new List<String>();

            // Check if we have a project open.
            if ((settingObject.SearchProject || settingObject.SearchInProjectOnly) && PluginBase.CurrentProject != null)
            {
                String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
                folders.Add(projectFolder);
            }

            return folders;
        }

        public List<String> GetSearchDirectories()
        {            
            List<String> folders = new List<String>();
            // Lots of things can go wrong here. Missing Permissions for example.
            try
            {
                // Check if we have a project open.
                if ((settingObject.SearchProject || settingObject.SearchInProjectOnly) && PluginBase.CurrentProject != null)
                {
                    bool bIncludeProjectFolders = true;
                    String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);

                    if (settingObject.SearchDirectory != settingObject.DefaultEmptyString)
                    {
                        if (settingObject.SearchDirectory.StartsWith(projectFolder))
                            bIncludeProjectFolders = false;
                    }

                    if (bIncludeProjectFolders)
                    {
                        folders.Add(projectFolder);
                        foreach (String path in PluginBase.CurrentProject.SourcePaths)
                        {
                            if (Path.IsPathRooted(path))
                            {
                                folders.Add(path);
                            }
                            else
                            {
                                String folder = Path.GetFullPath(Path.Combine(projectFolder, path));
                                if (!folder.StartsWith(projectFolder))
                                    folders.Add(folder);
                            }
                        }
                    }
                }

                // A directory was specified, lets get the underlying folders there!
                if (settingObject.SearchDirectory != settingObject.DefaultEmptyString)
                {
                    string[] directories = settingObject.SearchDirectory.Split(new char[] { ',', ';' });
                    if (directories.Length > 0 && directories[0] != settingObject.DefaultEmptyString)
                    {
                        foreach (string dir in directories)
                        {
                            if (Directory.Exists(dir))
                                folders.Add(dir);
                            else
                                MessageBox.Show("Path: \"" + dir + "\" not found. Please check your Find Files settings.", "Directory Not Found", MessageBoxButtons.OK);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception!", MessageBoxButtons.OK);
                settingObject.SearchDirectory = settingObject.DefaultEmptyString;
                Dispose();
            }                                 
            
            return folders;
        }

        public List<String> GetFiles(bool inProjectOnly = false)
        {
            if (cachedFiles.Count == 0)
            {
                List<String> folders = inProjectOnly ? GetProjectDirectory() : GetSearchDirectories();
                foreach (String folder in folders)
                {
                    try
                    {
                        string[] filters = settingObject.SearchFilter.Split(new char[] { ',', ';', '|' });
                        foreach (string filter in filters)
                        {
                            cachedFiles.AddRange(Directory.GetFiles(folder, filter, SearchOption.AllDirectories));
                        }
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("Search Filter: \"" + settingObject.SearchFilter + "\" is invalid. Visit http://msdn.microsoft.com/en-us/library/ms143316.aspx for more information.", "Invalid Search Filter", MessageBoxButtons.OK);
                        settingObject.SearchFilter = settingObject.DefaultSearchFilter;
                        return new List<String>();
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // Do nothing, did not find a folder, just don't include it.
                    }
                }
            }

            return cachedFiles;
        }
        #endregion

    }	
}
