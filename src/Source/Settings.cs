using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace FindFilesPlugin
{
	[Serializable]
	public class Settings
	{
		private const StringMatchingAlgorithms DEFAULT_SEARCH_ALGORITHM = StringMatchingAlgorithms.SmithWaterman;
		private const String  SHORTCUTS_CATEGORY                            = "Shortcuts";
		private const String  FINDFILES_CATEGORY                            = "Find Files";
		private const String  ADVANCED_CATEGORY                             = "Advanced";
		public  const String  DEFAULT_SEARCHDIRECTORY                       = "";
		private const String  DEFAULT_PRIORITIZED_PATTERN                   = "";
		public  const String  DEFAULT_SEARCHFILTER                          = "*.*";
		private const Boolean DEFAULT_CTRL_CLICK_ENABLED                    = true;
		private const Boolean DEFAULT_NORMALIZE_SEARCH                      = false;
		private const Boolean DEFAULT_SEARCH_PROJECT                        = true;
		private const Boolean DEFAULT_SEARCH_DIRECTORIES					= false;
		private const Boolean DEFAULT_IGNORE_CASE                           = false;
		private const Boolean DEFAULT_APPLY_PRIORITIZED_PATTERN_TO_FILEPATH = true;
		public  const double  DEFAULT_NORMALIZED_SEARCH_THRESHOLD           = 0.5;
		private const double  DEFAULT_PATTERN_MATCH_SIMILARITY_BOOST        = 0.1;
		private const double  DEFAULT_IN_PROJECT_DIR_SIMILARITY_BOOST       = 0.0;
		private const int     DEFAULT_MAX_RESULTS_SHOWN                     = 25;
		private const Keys    DEFAULT_RESOURCE_SHORTCUT                     = Keys.Alt | Keys.Shift | Keys.O;
		private const Keys    DEFAULT_OUTLINE_SHORTCUT                      = Keys.Control | Keys.Shift | Keys.O;

		private bool searchInProjectOnly;
		private Size findFilesFormSize;
		private Size outlineFormSize;
		private StringMatchingAlgorithms stringMatchingAlgorithm           = DEFAULT_SEARCH_ALGORITHM;
		private String searchDirectory                                     = DEFAULT_SEARCHDIRECTORY;
		private String excludedDirectories                                 = DEFAULT_SEARCHDIRECTORY;
		private String searchFilter                                        = DEFAULT_SEARCHFILTER;
		private String prioritizedPattern                                  = DEFAULT_PRIORITIZED_PATTERN;
		private Keys openResourceShortcut                                  = DEFAULT_RESOURCE_SHORTCUT;
		private Keys quickOutlineShortcut                                  = DEFAULT_OUTLINE_SHORTCUT;
		private Boolean showDirectories									   = DEFAULT_SEARCH_DIRECTORIES;
		private Boolean searchProject                                      = DEFAULT_SEARCH_PROJECT;
		private Boolean ignoreCase                                         = DEFAULT_SEARCH_PROJECT;
		private Boolean applyPrioritizedPatternToFilepath                  = DEFAULT_APPLY_PRIORITIZED_PATTERN_TO_FILEPATH;
		private Boolean ctrlClickEnabled                                   = DEFAULT_CTRL_CLICK_ENABLED;
		private Boolean normalizeSearchResults                             = DEFAULT_NORMALIZE_SEARCH;
		private double patternMatchSimilarityBoost                         = DEFAULT_PATTERN_MATCH_SIMILARITY_BOOST;
		private double inProjectDirectorySimilarityBoost                   = DEFAULT_IN_PROJECT_DIR_SIMILARITY_BOOST;
		private double normalizedSearchThreshold                           = DEFAULT_NORMALIZED_SEARCH_THRESHOLD;
		private int maxResultsShown                                        = DEFAULT_MAX_RESULTS_SHOWN;
		private int fileNameWidth                                          = 0;
		private int filePathWidth                                          = 0;

		#region Hidden Plugin Settings

		[Browsable(false)]
		public Size FindFilesFormSize
		{
			get { return findFilesFormSize; }
			set { findFilesFormSize = value; }
		}
		[Browsable(false)]
		public Size OutlineFormSize
		{
			get { return outlineFormSize; }
			set { outlineFormSize = value; }
		}

		[Browsable(false)]
		public int FileNameWidth
		{
			get { return fileNameWidth; }
			set { fileNameWidth = value; }
		}
		[Browsable(false)]
		public int FilePathWidth
		{
			get { return filePathWidth; }
			set { filePathWidth = value; }
		}
		[Browsable(false)]
		public bool SearchInProjectOnly
		{
			get { return searchInProjectOnly; }
			set { searchInProjectOnly = value; }
		}

		[Browsable(false)]
		public String DefaultEmptyString { get { return DEFAULT_SEARCHDIRECTORY; } }
		[Browsable(false)]
		public String DefaultSearchFilter { get { return DEFAULT_SEARCHFILTER; } }
		#endregion

		#region ADVANCED SETTINGS

		[Category(ADVANCED_CATEGORY)]
		[DisplayName("Normalized Search Threshold")]
		[Description("A threshold that determines when a string is similar enough to get shown in the Find Window. Lower numbers result in more, but less accurate results. Must have \"Normalize Search Results\" enabled in oder to have an effect.")]
		[DefaultValue(DEFAULT_NORMALIZED_SEARCH_THRESHOLD)]
		public Double NormalizedSearchThreshold
		{
			get
			{
				if (normalizedSearchThreshold < 0 || normalizedSearchThreshold > 1)
				{
					MessageBox.Show("Search Threshold needs to be a floating point number between 0 and 1. Threshold Reset to 0.5.", "Invalid Search Threshold", MessageBoxButtons.OK);
					normalizedSearchThreshold = DEFAULT_NORMALIZED_SEARCH_THRESHOLD;
				}
				return normalizedSearchThreshold;
			}
			set { normalizedSearchThreshold = value; }
		}

		[Category(ADVANCED_CATEGORY)]
		[DisplayName("Normalize Search Results")]
		[Description("Checks accuracy of words based on a scale of 0-1. Setting this to True will allow results to be discarded if their similarity boost is less than the specified \"Normalied Search Threshold\". This results in better performance at the cost of losing potential matches.")]
		[DefaultValue(DEFAULT_NORMALIZE_SEARCH)]
		public Boolean NormalizeSearchResults
		{
			get { return normalizeSearchResults; }
			set { normalizeSearchResults = value; }
		}

		[Category(ADVANCED_CATEGORY)]
		[DisplayName("Ignore Case")]
		[Description("Causes file names to be lowercased before being compared with the search term.")]
		[DefaultValue(DEFAULT_IGNORE_CASE)]
		public Boolean IgnoreCase
		{
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		[Category(ADVANCED_CATEGORY)]
		[DisplayName("Apply Prioritized Pattern to File Path")]
		[Description("Setting this to False will cause the program to search for the\"Prioritized String Patterns\" on the File Name (including extension) only, instead of the whole File Path. Set to False for slightly better performance.")]
		[DefaultValue(DEFAULT_APPLY_PRIORITIZED_PATTERN_TO_FILEPATH)]
		public Boolean ApplyPrioritizedPatternToFilepath
		{
			get { return applyPrioritizedPatternToFilepath; }
			set { applyPrioritizedPatternToFilepath = value; }
		}

		[Category(ADVANCED_CATEGORY)]
		[DisplayName("Similarity Boost - Pattern Match")]
		[Description("Value that gets added to the similarity of files that contain a prioritized pattern. Increasing this number makes files that contain a prioritized pattern in their name more likely to show up first in the search results.")]
		[DefaultValue(DEFAULT_PATTERN_MATCH_SIMILARITY_BOOST)]
		public double PatternMatchSimilarityBoost
		{
			get { return patternMatchSimilarityBoost; }
			set { patternMatchSimilarityBoost = value; PluginMain.ClearCachedFiles(); }
		}

		[Category(ADVANCED_CATEGORY)]
		[DisplayName("Similarity Boost - Within Project")]
		[Description("Only valid if \"Search Project\" is set to True - Value that gets added to the similarity of files that are in the current project. Increasing this number makes files in the project more likely to show up first in the search results.")]
		[DefaultValue(DEFAULT_IN_PROJECT_DIR_SIMILARITY_BOOST)]
		public double InProjectDirectorySimilarityBoost
		{
			get { return inProjectDirectorySimilarityBoost; }
			set { inProjectDirectorySimilarityBoost = value; PluginMain.ClearCachedFiles(); }
		}
		#endregion

		#region FINDFILES SETTINGS

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("Search Directories")]
		[Description("If this is set the program will look for files in the specified directories, separated by delimiter (',' ';'). ONLY ABSOLUTE PATHS!")]
		[DefaultValue(DEFAULT_SEARCHDIRECTORY)]
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
		public String SearchDirectory
		{
			get { return searchDirectory; }
			set { searchDirectory = value; PluginMain.ClearCachedFiles(); }
		}

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("Excluded Directories")]
		[Description("A list of excluded directories, separated by delimiter (',' ';'). ONLY ABSOLUTE PATHS!")]
		[DefaultValue(DEFAULT_SEARCHDIRECTORY)]
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
		public String ExcludedDirectories
		{
			get { return excludedDirectories; }
			set { excludedDirectories = value; PluginMain.ClearCachedFiles(); }
		}  

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("File Type Filter")]
		[Description("Search for files with a specific extension. Separate extensions with ',' for multiple file type search. Extensions should be of format   *.ext  ")]
		[DefaultValue(DEFAULT_SEARCHFILTER)]
		public String SearchFilter
		{
			get { return searchFilter; }
			set { searchFilter = value; PluginMain.ClearCachedFiles(); }
		}

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("Search Project")]
		[Description("Search for files in the current project in addition to the specified \"Search Directories\" (only valid if a project is opened). ")]
		[DefaultValue(DEFAULT_SEARCH_PROJECT)]
		public Boolean SearchProject
		{
			get { return  searchProject; }
			set { searchProject = value; PluginMain.ClearCachedFiles(); }
		}

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("Show Directories")]
		[Description("Also show directories in search results (directory name used for matching, will open windows explorer if selected).")]
		[DefaultValue(DEFAULT_SEARCH_DIRECTORIES)]
		public Boolean ShowDirectories
		{
			get { return showDirectories; }
			set { showDirectories = value; PluginMain.ClearCachedFiles(); }
		}

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("Prioritized String Patterns")]
		[Description("If a file name contains the above text, it will get a priority boost (adjustable). File extensions can be prioritized if the \"Remove File Extension\" setting is disabled. (SEPARATE MULTIPLES WITH ',')")]
		[DefaultValue(DEFAULT_PRIORITIZED_PATTERN)]
		public String PrioritizedPattern
		{
			get { return prioritizedPattern; }
			set { prioritizedPattern = value; PluginMain.ClearCachedFiles(); }
		}

		[Category(FINDFILES_CATEGORY)]
		[DisplayName("String Matching Algorithm")]
		[Description("Select the algorithm used to find the best match for your search. Try them out to find the one that gets the best results with good performance for your situation.")]
		[DefaultValue(DEFAULT_SEARCH_ALGORITHM)]
		public StringMatchingAlgorithms StringMatchingAlgorithm
		{
			get { return stringMatchingAlgorithm; }
			set { stringMatchingAlgorithm = value; }
		}
		#endregion

		#region MISC SETTINGS
		// MISC Category
		[DisplayName("Enable navigation by Ctrl+Click")]
		[Description("Go to declaration by Ctrl+Click on the word")]
		[DefaultValue(DEFAULT_CTRL_CLICK_ENABLED)]
		public Boolean CtrlClickEnabled
		{
			get { return ctrlClickEnabled; }
			set { ctrlClickEnabled = value; }
		}

		// MISC Category
		[DisplayName("Max Results Shown")]
		[Description("Maximum number of results shown in find windows. Valid range is 1 - 100")]
		[DefaultValue(DEFAULT_MAX_RESULTS_SHOWN)]
		public int MaxResultsShown
		{
			get
			{
				if (maxResultsShown <= 1 || maxResultsShown > 100)
				{
					MessageBox.Show("\"Max Results Show\" valid range is between 1 - 100. Value reset to default [25].", "FindFiles Error", MessageBoxButtons.OK);
					maxResultsShown = DEFAULT_MAX_RESULTS_SHOWN;
				}
				return maxResultsShown;
			}
			set { maxResultsShown = value; }
		}
		#endregion

		#region SHORTCUTS SETTINGS

		[Category(SHORTCUTS_CATEGORY)]
		[DisplayName("Find Files")]
		[Description("Shortcut to open the Find Files dialog.")]
		[DefaultValue(DEFAULT_RESOURCE_SHORTCUT)]
		[Browsable(false)] // Shortcuts now set using the shortcut manager (FD4 way)
		public Keys OpenResourceShortcut
		{
			get { return openResourceShortcut; }
			set { openResourceShortcut = value; }
		}

		[Category(SHORTCUTS_CATEGORY)]
		[DisplayName("Quick Outline")]
		[Description("Shortcut to open QuickOutline dialog")]
		[DefaultValue(DEFAULT_OUTLINE_SHORTCUT)]
		[Browsable(false)] // Shortcuts now set using the shortcut manager (FD4 way)
		public Keys QuickOutlineShortcut
		{
			get { return quickOutlineShortcut; }
			set { quickOutlineShortcut = value; }
		}
		#endregion

	}

	public enum StringMatchingAlgorithms
	{
		SmithWatermanGotoh,
		SmithWaterman,
		JaroWinkler,
		Jaro
	}
}