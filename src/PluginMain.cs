using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;

namespace QuickNavigatePlugin
{
	public class PluginMain : IPlugin
	{
        private const String PLUGIN_NAME = "QuickNavigate";
        private const String PLUGIN_GUID = "ac04a177-f578-47d7-87f1-0cbc0f834446";
        private const String PLUGIN_HELP = "www.flashdevelop.org/community/";
        private const String PLUGIN_AUTH = "Canab & CrazySam";
	    private const String SETTINGS_FILE = "Settings.fdb";
        private const String PLUGIN_DESC = "QuickNavigate plugin";

        private String settingFilename;
        private Settings settingObject;
	    private ControlClickManager controlClickManager;

        private ToolStripMenuItem findFilesMenuItem = null;
        private ToolStripMenuItem quickOutlineMenuItem = null;

	    #region Required Properties

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
		
		/// <summary>
		/// Handles the incoming events
		/// </summary>
        public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
		{
            if (e.Type == EventType.FileSwitch)
            {
                if (controlClickManager != null)
                    controlClickManager.SciControl = PluginBase.MainForm.CurrentDocument.SciControl;                
            }
            else if (e.Type == EventType.ApplySettings)
            {                
                if (!PluginBase.MainForm.IgnoredKeys.Contains(settingObject.OpenResourceShortcut))                
                    PluginBase.MainForm.IgnoredKeys.Add(settingObject.OpenResourceShortcut);

                if (!PluginBase.MainForm.IgnoredKeys.Contains(settingObject.QuickOutlineShortcut))
                    PluginBase.MainForm.IgnoredKeys.Add(settingObject.QuickOutlineShortcut);
                
                findFilesMenuItem.ShortcutKeys = settingObject.OpenResourceShortcut;
                quickOutlineMenuItem.ShortcutKeys = settingObject.QuickOutlineShortcut;
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
            EventManager.AddEventHandler(this, EventType.ApplySettings);
        }

        public void CreateMenuItems()
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("ViewMenu");            
            findFilesMenuItem = new ToolStripMenuItem("Find Files",
                PluginBase.MainForm.FindImage("209"),
                new EventHandler(ShowResourceForm),
                settingObject.OpenResourceShortcut);

            quickOutlineMenuItem = new ToolStripMenuItem("Quick Outline",
            PluginBase.MainForm.FindImage("315|16|0|0"),
            new EventHandler(ShowOutlineForm),
            settingObject.QuickOutlineShortcut);


            menu.DropDownItems.Add(findFilesMenuItem);
            menu.DropDownItems.Add(quickOutlineMenuItem);

            PluginBase.MainForm.IgnoredKeys.Add(settingObject.OpenResourceShortcut);
            PluginBase.MainForm.IgnoredKeys.Add(settingObject.QuickOutlineShortcut);
        }        

        private void ShowResourceForm(object sender, EventArgs e)
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

        public List<String> GetProjectFiles()
        {
            List<String> folders = GetProjectFolders();
            List<String> files = new List<string>();
            foreach (String folder in folders)
            {
                try
                {
                    string[] filters = settingObject.SearchFilter.Split(',');
                    foreach (string filter in filters)
                    {
                        files.AddRange(Directory.GetFiles(folder, filter, SearchOption.AllDirectories));   
                    }                    
                }
                catch(ArgumentException)
                {
                    MessageBox.Show("Search Filter : \"" + settingObject.SearchFilter + "\" is invalid. Visit http://msdn.microsoft.com/en-us/library/ms143316.aspx for more information.", "Invalid Search Filter", MessageBoxButtons.OK);
                    settingObject.SearchFilter = settingObject.DefaultSearchFilter;
                    return new List<String>();
                }
            }
            return files;
        }

        public List<String> GetProjectFolders()
        {            
            List<String> folders = new List<String>();
            // Lots of things can go wrong here. Missing Permissions for example.
            try
            {
                // Check if we have a project open.
                if (PluginBase.CurrentProject != null)
                {
                    String projectFolder = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
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

                // A directory was specified, lets get the underlying folders there!
                if (settingObject.SearchDirectory != settingObject.DefaultSearchDirectory)
                {
                    if (!Directory.Exists(settingObject.SearchDirectory))
                    {
                        MessageBox.Show("Path: \"" + settingObject.SearchDirectory + "\" not found.", "Directory Not Found", MessageBoxButtons.OK);
                        settingObject.SearchDirectory = settingObject.DefaultSearchDirectory;
                        return folders;
                    }
                    folders.Add(settingObject.SearchDirectory);
                } 
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception!", MessageBoxButtons.OK);
                settingObject.SearchDirectory = settingObject.DefaultSearchDirectory;
                Dispose();
            }                                 
            
            return folders;
        }
		#endregion

	}	
}
