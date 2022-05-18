using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public static class Menu
    {
        static private readonly TooltipGenerator tooltipGenerator = new TooltipGenerator();

        #region editor settings
        static private readonly Settings settings = new Settings();
        static public Settings Settings => settings;

        #endregion

        #region files and folders panel

        /// <summary>
        /// The folder that is displayed when opening a file or folder panel
        /// </summary>
        private static string initialFolderPath
        {
            get { return EditorPrefs.GetString(initialFolderPathPreferenceKey, Application.dataPath); }
            set { EditorPrefs.SetString(initialFolderPathPreferenceKey, value); }
        }
        private const string initialFolderPathPreferenceKey = "C2T_InitialFolderPath";

        private const string supportedFileExtension = "cs";

        #endregion

        #region batch info

        private static int batchFilesCount;
        private static int batchProcessedFilesCount;

        #endregion

        #region ui texts

        //menu items
        private const string menuName = "Tools/Comment To Tooltip";
        private const string processFileMenuItem = "Process a file...";
        private const string processFolderMenuItem = "Process a folder...";

        private const string aboutMenuItem = "Help";
        private const string settingsMenuItem = "Settings";

        //windows titles
        private const string fileSelectionTitle = "Select a file";
        private const string folderSelectionTitle = "Select a folder";
        private const string processingInProgressTitle = "File processing";
        private const string settingsWindowTitle = "Settings";

        //windows content
        private const string helpFileNotFoundDialogMessage = "Help can be found in the ReadMe.txt file";
        private const string noCommentTypesSelectedDialogMessage = "No comment types selected. Please select at least one in the {0} menu.";
        private const string noFilesFoundDialogMessage = "No files found to process.";
        private const string processingInProgressDialogMessage = "Processing file {0} out of {1} : {2}";
        private const string processingInteruptedDialogMessage = "The operation was stopped by the user. No changes were applied.";

        private const string processingSuccededDialogMessage = "Processing of {0} file{1} completed successfully in {2}s. ";
        private const string processingSucceededWithRecoverableErrorDialogMessage = "Errors were encountered for {0} ignored. ";
        private const string processingSucceededWithErrorSingular = "one file. That file was";
        private const string processingSucceededWithErrorPlural = "{0} files. These files were";
            
        private const string processingSuccededFilesModifiedDialogMessage = "{0} file{1} modified. ";
        private const string processingSuccededNoFilesModifiedDialogMessage = "No files were modified. ";

        private const string processingSucceesedSeeLogsDialogMessageConsoleOnly = "You can find more details in the console";
        private const string processingSuccededSeeLogsDialogMessage = "You can find more details in the console or in the logs file.";

        private const string processingEncountredCriticalErrorDialogMessage = "An unexpected error occurred while processing a file. No changes were applied. You can find more details in the console.";
        private const string okButtonDialogBox = "Ok";

        //console messages
        private const string filesModifiedConsoleMessage = "Tooltips added or modified in the following files:";
        private const string logsUpdatedConsoleMessage = "Messages logged in the following file: '{0}'.";
        private const string recoverableErrorEncoutredConsoleMessage = "An exception occured while processing the following file '{0}'. This file will be ignored.";

        //log file only messages
        private const string batchStartedLogMessage = "Started processing file(s).";

        #endregion

        private static string LogsFilePath => $"{Application.dataPath}/{ToolName}_logs.txt";
        static public string ToolName => "Comment To Tooltip";

        static Menu()
        {
            settings.ReadFromEditorPreferences();
        }

        #region menu items

        [MenuItem(menuName + "/" + processFileMenuItem)]
        public static void OnProcessFile()
        {
            string filePath = EditorUtility.OpenFilePanel(fileSelectionTitle, initialFolderPath, supportedFileExtension);
            if (File.Exists(filePath))
            {
                initialFolderPath = new FileInfo(filePath).Directory.FullName;
                ProcessFiles(new[] { filePath });
            }
        }

        [MenuItem(menuName + "/" + processFolderMenuItem)]
        public static void OnProcessAFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel(folderSelectionTitle, initialFolderPath, "");
            if (Directory.Exists(folderPath))
            {
                initialFolderPath = folderPath;
                string[] filePaths = Directory.GetFiles(folderPath, $"*.{supportedFileExtension}", SearchOption.AllDirectories);
                ProcessFiles(filePaths);
            }
        }

        [MenuItem(menuName + "/" + settingsMenuItem)]
        public static void OnSettings()
        {
            EditorWindow.GetWindow<SettingsWindow>(true, settingsWindowTitle, true);
        }

        [MenuItem(menuName + "/" + aboutMenuItem)]
        public static void OnHelp()
        {
            DisplayDialogBoxMessage(helpFileNotFoundDialogMessage, false);
        }

        #endregion

        #region processing files

        private static void ProcessFiles(string[] filePaths)
        {
            CommentTypes commentTypes = Settings.GetCommenTypes();

            List<string> validatedFilePaths = filePaths.AsEnumerable().
                Where(s => string.IsNullOrEmpty(s) == false).ToList();

            var logToFile = Settings.EnableLoggingToFile;

            DisplayConsoleMessage(batchStartedLogMessage, logToFile);

            if (validatedFilePaths.Count == 0)
            {
                DisplayDialogBoxMessage(noFilesFoundDialogMessage, logToFile);
            }
            else if (commentTypes == CommentTypes.None)
            {
                DisplayDialogBoxMessage(string.Format(CultureInfo.InvariantCulture, noCommentTypesSelectedDialogMessage, settingsMenuItem), logToFile);
            }
            else
            {
                //start batch
                batchFilesCount = validatedFilePaths.Count;
                batchProcessedFilesCount = 0;

                List<KeyValuePair<string, string>> filesToWrite = new List<KeyValuePair<string, string>>();
                try
                {
                    filesToWrite = GetModifiedFiles(validatedFilePaths, commentTypes);
                }
                catch (Exception)
                {
                    filesToWrite.Clear();
                    DisplayDialogBoxMessage(processingEncountredCriticalErrorDialogMessage, logToFile);
                    throw;
                }
                finally
                {
                    WriteFilesToDisk(filesToWrite);
                    LogModifiedFilesList(filesToWrite);
                    EditorUtility.ClearProgressBar();
                    //end batch
                    batchFilesCount = 0;
                    batchProcessedFilesCount = 0;
                }
            }
        }

        /// <summary>
        /// Returns the files that were modified to add/update tooltips.
        /// This method doesn't write on disk the modified files
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="commentTypes"></param>
        /// <returns>A list of pairs. The pair's key is the file's path, and the pair's value is the modified file's content</returns>
        private static List<KeyValuePair<string, string>> GetModifiedFiles(List<string> filePaths, CommentTypes commentTypes)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            List<KeyValuePair<string, string>> modifiedFiles =
                new List<KeyValuePair<string, string>>(batchFilesCount);

            int exceptionCount = 0;

            bool operationWasCanceled = false;

            var logToFile = Settings.EnableLoggingToFile;

            foreach (string filePath in filePaths)
            {
                //canceling handling
                if (DisplayCancelableProgressBar(filePath))
                {
                    operationWasCanceled = true;
                    break;
                }

                try
                {
                    
                    //string readFileContent = File.ReadAllText(filePath, Encoding.Default);
                    if (tooltipGenerator.TryProcessText(
                            Path.GetExtension(filePath),
                            File.ReadAllText(filePath, Encoding.Default), //readFileContent, 
                            out string modifiedFileContent,
                            commentTypes
                        )
                    )
                        modifiedFiles.Add(new KeyValuePair<string, string>(filePath, modifiedFileContent));
                }
                catch (Exception exception)
                {
                    DisplayConsoleMessage(string.Format(recoverableErrorEncoutredConsoleMessage, filePath), logToFile);
                    DisplayConsoleException(exception, logToFile);
                    exceptionCount++;
                }
                finally
                {
                    batchProcessedFilesCount++;
                }
            }

            if (operationWasCanceled)
            {
                modifiedFiles.Clear();
                DisplayDialogBoxMessage(processingInteruptedDialogMessage, logToFile);
            }
            else
                DisplayOperationCompletedMessage(modifiedFiles.Count, stopWatch.Elapsed.TotalSeconds, exceptionCount);

            stopWatch.Stop();

            return modifiedFiles;
        }

        private static bool DisplayCancelableProgressBar(string fileName)
        {
            return EditorUtility.DisplayCancelableProgressBar(
                processingInProgressTitle,
                string.Format(CultureInfo.InvariantCulture, processingInProgressDialogMessage,
                (batchProcessedFilesCount + 1), batchFilesCount, fileName),
                (float)batchProcessedFilesCount / batchFilesCount);
        }

        private static void LogModifiedFilesList(List<KeyValuePair<string, string>> filesToWrite)
        {
            if (filesToWrite.Count != 0)
            {
                var logToFile = Settings.EnableLoggingToFile;
                DisplayConsoleMessage(string.Format(CultureInfo.InvariantCulture, filesModifiedConsoleMessage), logToFile);
                foreach (string file in filesToWrite.Select(pair => pair.Key).ToList())
                    DisplayConsoleMessage(file, logToFile);
                
                if (logToFile) // won't tell the user if stuff has been logged to a file if it's not logging things to a file.
                    Debug.Log(string.Format(CultureInfo.InvariantCulture, logsUpdatedConsoleMessage, LogsFilePath));
            }
        }

        private static void WriteFilesToDisk(List<KeyValuePair<string, string>> filesToWrite)
        {
            foreach (KeyValuePair<string, string> pair in filesToWrite)
            {
                using (StreamWriter writer = new StreamWriter(pair.Key, false, Encoding.Default))
                {
                    writer.Write(pair.Value);
                }
            }
        }

        private static void DisplayOperationCompletedMessage(
            int writtenFilesCount, double operationDurationInSeconds, int exceptionCount
        )
        {
            string operationCompletedMessage = string.Format(
                CultureInfo.InvariantCulture,
                processingSuccededDialogMessage,
                batchProcessedFilesCount,
                batchProcessedFilesCount == 1 ? string.Empty : "s",
                operationDurationInSeconds.ToString("F", CultureInfo.InvariantCulture)
            );

            if (exceptionCount > 0)
            {
                if (exceptionCount == 1) // if one exception happened, 'files ignored' message is singular
                    operationCompletedMessage += string.Format(
                        CultureInfo.InvariantCulture,
                        processingSucceededWithRecoverableErrorDialogMessage,
                        processingSucceededWithErrorSingular
                    );
                else // if more than one exception happened, 'files ignored' is plural, and says how many were ignored
                    operationCompletedMessage += string.Format(
                        CultureInfo.InvariantCulture,
                        processingSucceededWithRecoverableErrorDialogMessage,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            processingSucceededWithErrorPlural,
                            exceptionCount.ToString("d", CultureInfo.InvariantCulture)
                        )
                    );
            }

            if (writtenFilesCount == 0)
                operationCompletedMessage += processingSuccededNoFilesModifiedDialogMessage;
            else
                operationCompletedMessage += string.Format(
                    CultureInfo.InvariantCulture,
                    processingSuccededFilesModifiedDialogMessage, writtenFilesCount,
                    writtenFilesCount == 1 ? string.Empty : "s");

            if (writtenFilesCount > 0 || exceptionCount > 0) 
                // only told to see log file if the log file is enabled for this situation
                operationCompletedMessage += (writtenFilesCount > 0 && Settings.EnableLoggingToFile) ||
                                             (exceptionCount > 0 && Settings.EnableLoggingExceptionsToFile)
                    ? processingSuccededSeeLogsDialogMessage
                    : processingSucceesedSeeLogsDialogMessageConsoleOnly;
            
            DisplayDialogBoxMessage(operationCompletedMessage);
        }

        #endregion

        #region messages displaying

        private static void DisplayDialogBoxMessage(string message)
        {
            DisplayDialogBoxMessage(message, Settings.EnableLoggingToFile);
        }

        private static void DisplayDialogBoxMessage(string message, bool logToFile)
        {
            if (logToFile)
                _LogToFile(message);
            EditorUtility.DisplayDialog(ToolName, message, okButtonDialogBox);
        }

        private static void DisplayConsoleMessage(string message)
        {
            DisplayConsoleMessage(message, Settings.EnableLoggingToFile);
        }

        private static void DisplayConsoleMessage(string message, bool logToFile)
        {
            if (logToFile)
                _LogToFile(message);
            Debug.Log(message);
        }

        private static void DisplayConsoleException(Exception exception)
        {
            DisplayConsoleException(exception, Settings.EnableLoggingToFile);
        }
        
        /// <summary>
        /// handles displaying an exception to the console. Will log it to the file as well,
        /// if <paramref name="defaultLogToFile"/> or if <code>Settings.EnableLoggingExceptionsToFile</code>
        /// are true.
        /// </summary>
        /// <param name="exception">the exception to log</param>
        /// <param name="defaultLogToFile">THIS SHOULD BE 'Settings.EnableLoggingToFile'</param>
        /// <seealso cref="Settings"/>
        private static void DisplayConsoleException(Exception exception, bool defaultLogToFile)
        {
            if (defaultLogToFile || Settings.EnableLoggingExceptionsToFile)
                _LogToFile(exception.ToString());
            Debug.LogException(exception);
        }

        /// <summary>
        /// accessible wrapper for <see cref="_LogToFile"/>, validating whether or not logging to file is allowed.
        /// If Settings.EnableLoggingToFile is true, this will forward <paramref name="message"/> to the
        /// actual <see cref="_LogToFile"/> method (allowing it to be logged to file)
        /// </summary>
        /// <param name="message">The message which may or may not get logged to the file.</param>
        private static void LogToFile(string message)
        {
            if (Settings.EnableLoggingToFile) _LogToFile(message);
        }
        
        private static void _LogToFile(string message)
        {
            using (StreamWriter writer = new StreamWriter(LogsFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now} : {message}");
            }
        }

        #endregion
    }
}
