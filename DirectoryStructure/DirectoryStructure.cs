using System;
using System.IO;
using System.Collections.Generic;

namespace DirectoryManagement
{
    public static class DirectoryStructure
    {
        #region Properties
        private static List<DirectoryElement> elementLibrary = new List<DirectoryElement>();        //Stores the element paths of the structure (directories and files)
        private static List<DirectoryElement> removeFromLibrary = new List<DirectoryElement>();
        private static string rootDirectory = string.Empty;     //Name of the root directory
        private static bool inteliDirectory = false;
        private static bool baseSet = false;    //True, if baseDirectory has been set
        private static bool reset = false;      //If true, 'RepairStructure' will reset the directory system

        private static int deleteCallCount = 0;     //Counts the calls in a Delete cycle, so the actual erase will appear only at the end of the cycle

        /// <summary>
        /// Returns the root directory path
        /// </summary>
        public static string root { get { return rootDirectory; } }
        /// <summary>
        /// Returns the state of the structure. (Has been set or not.)
        /// </summary>
        public static bool active { get { return baseSet; } }

        public delegate void Foreach(DirectoryElement directoryElement);
        #endregion

        #region Basic Management
        /// <summary>
        /// This function needs to be called first. This will be added to every newly created directory and file.
        /// </summary>
        /// <param name="rootDirName">Defines the name of the root directory (usually like: Application.dataPath) of your costum files (such as game save files).</param>
        /// <param name="intelDir">When set to true, the system will be set to Intelligent Directory mode. With this turned on, you don't have to add every directory by yourself. When you add a file (or directory), the system will register every element of that file's path into the library. Remember, with this, you will lose the opportunity to give nickname to those directories. If you want to do so, then first add that specific directory and then add the files within it.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo SetupStructure(string rootDirName, bool intelDir)
        {
            if (!baseSet)
            {
                rootDirectory = rootDirName;
                inteliDirectory = intelDir;
                baseSet = true;
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.AlreadySet;
        }

        /// <summary>
        /// Tries to add the given directory (or file) path to the structure and returns the result of the operation.
        /// </summary>
        /// <param name="givenPath">The path to the directory or file. Separate the elements (directories and files) with '/' character. Example: MyDirectory/SaveFiles/save.txt</param>
        /// <param name="force">If forced, it overwrites the directory/file with the same name.</param>
        /// <param name="isFile">Set it true if givenPath means the path to a file.</param>
        /// <param name="nickname">Optional parameter. Use it to later refer to the path by sending a nickname into the "GetDirectory" method.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo Add(string givenPath, bool force, bool isFile, string nickname = null)
        {
            if (baseSet)        //If directory has been set up
            {
                try
                {
                    //Variables
                    string[] elements = givenPath.Split('/');
                    string path = rootDirectory;

                    //Checks the directories in path and registers them into the library
                    if (inteliDirectory)
                        LibraryFill(givenPath);

                    //If directory
                    if (!isFile)
                    {
                        //Creates path and add it to elementLibrary
                        for (int i = 0; i < elements.Length - 1; i++) { path = Path.Combine(path, elements[i]); }
                        string dirPath = Path.Combine(path, elements[elements.Length - 1]);

                        if (Check(dirPath, false) == DirectoryOperationInfo.Success)
                        {
                            if (Directory.Exists(dirPath))
                            {
                                if (force)
                                {
                                    //Deletes everything
                                    Delete(dirPath);

                                    //Reset the directory
                                    CreateElement(dirPath, path);
                                    return DirectoryOperationInfo.Success;
                                }
                                return DirectoryOperationInfo.ElementAlreadyExists;
                            }
                            else
                            {
                                Directory.CreateDirectory(dirPath);
                                return DirectoryOperationInfo.Success;
                            }
                        }
                        else
                        {
                            if (Directory.Exists(dirPath))
                            {
                                if (force)
                                {
                                    //Deletes everything
                                    Delete(dirPath);
                                    CreateElement(dirPath, path);
                                    return DirectoryOperationInfo.Success;
                                }
                                elementLibrary.Add(new DirectoryElement(dirPath, false, path, nickname, givenPath));
                                return DirectoryOperationInfo.Success;
                            }
                            else
                            {
                                CreateElement(dirPath, path);
                                return DirectoryOperationInfo.Success;
                            }
                        }
                    }

                    //If file
                    else
                    {
                        //Creates path
                        for (int i = 0; i < elements.Length - 1; i++) { path = Path.Combine(path, elements[i]); }
                        string filePath = Path.Combine(path, elements[elements.Length - 1]);

                        //If in library
                        if (Check(filePath, false) == DirectoryOperationInfo.Success)
                        {
                            if (Directory.Exists(path))     //If directory exists
                            {
                                if (File.Exists(filePath))       //And file exists
                                {
                                    if (force)      //And forced
                                    {
                                        Delete(filePath);
                                        CreateElement(filePath, path);
                                        return DirectoryOperationInfo.Success;
                                    }
                                    return DirectoryOperationInfo.ElementAlreadyExists;
                                }
                                else
                                {
                                    File.Create(filePath).Dispose();
                                    return DirectoryOperationInfo.Success;
                                }
                            }
                            else
                            {
                                Add(path, false, false, null);
                                //Create Directory
                                File.Create(filePath).Dispose();
                                return DirectoryOperationInfo.Success;
                            }
                        }

                        //If not
                        else
                        {
                            if (Directory.Exists(path))     //If directory exists
                            {
                                if (File.Exists(filePath))       //And file exists
                                {
                                    if (force)      //And forced
                                    {
                                        File.Delete(filePath);
                                        CreateElement(filePath, path);
                                        return DirectoryOperationInfo.Success;
                                    }
                                    elementLibrary.Add(new DirectoryElement(filePath, isFile, path, nickname, givenPath));
                                    return DirectoryOperationInfo.ElementAlreadyExists;
                                }
                                else
                                {
                                    CreateElement(filePath, path);
                                    return DirectoryOperationInfo.Success;
                                }
                            }
                            else
                            {
                                //Separate dir path
                                givenPath = elements[0];
                                for (int i = 1; i < elements.Length - 1; i++) { givenPath += "/" + elements[i]; }
                                //Create Directory
                                Directory.CreateDirectory(path);
                                elementLibrary.Add(new DirectoryElement(path, false, null, null, givenPath));
                                //Then create file
                                CreateElement(filePath, path);
                                return DirectoryOperationInfo.Success;
                            }
                        }
                    }
                }
                catch { return DirectoryOperationInfo.Failure; }
            }
            return DirectoryOperationInfo.NotSet;

            void CreateElement(string path, string dirPath = null)
            {
                if (!isFile)
                {
                    Directory.CreateDirectory(path);
                    elementLibrary.Add(new DirectoryElement(path, false, dirPath, nickname, givenPath));
                }
                else
                {
                    File.Create(path).Dispose();
                    elementLibrary.Add(new DirectoryElement(path, true, dirPath, nickname, givenPath));
                }
            }
        }

        /// <summary>
        /// Deletes a directory or file from the structure.
        /// </summary>
        /// <param name="clue">You can select a directory element by a clue (such as nickname) or by typing the specific path. The path elements (directories and files) must be separated by '/' charater. Example: MyDirectory/SaveFiles/save.txt</param>
        /// <returns></returns>
        public static DirectoryOperationInfo Delete(string clue)
        {
            if (baseSet)
            {
                //Variables
                DirectoryElement element = GetDirectory(clue);
                string path = null;
                if (element != null)
                    path = element.path;

                if (path != null)
                {
                    //If directory
                    if (!element.isFile)
                    {
                        if (path != null)
                        {
                            deleteCallCount++;
                            removeFromLibrary.Add(element);
                            foreach (DirectoryElement current in elementLibrary)
                            {
                                if (current.isFile && current.dirPath == path)
                                    removeFromLibrary.Add(current);
                                //RemoveFromLibrary(current.path);    //Delete files inside                            
                                else if (!current.isFile && current.dirPath == path)
                                    Delete(current.userPath);
                            }
                            deleteCallCount--;
                            try { Directory.Delete(path, true); } catch { return DirectoryOperationInfo.Failure; }
                            if (deleteCallCount == 0)
                            {
                                foreach (DirectoryElement current in removeFromLibrary) { elementLibrary.Remove(current); }
                                removeFromLibrary = new List<DirectoryElement>();
                            }
                            return DirectoryOperationInfo.Success;
                        }
                        return DirectoryOperationInfo.MissingElement;
                    }

                    //If file
                    else
                    {
                        if (path != null)
                        {
                            deleteCallCount++;
                            removeFromLibrary.Add(element);
                            try { File.Delete(path); } catch { }
                            deleteCallCount--;
                            if (deleteCallCount == 0)
                            {
                                elementLibrary.Remove(element);
                                removeFromLibrary = new List<DirectoryElement>();
                            }
                            return DirectoryOperationInfo.Success;
                        }
                        return DirectoryOperationInfo.MissingElement;
                    }
                }
                return DirectoryOperationInfo.MissingElement;
            }
            return DirectoryOperationInfo.NotSet;
        }

        /// <summary>
        /// Checks for the existance of a directory or file.
        /// </summary>
        /// <param name="clue">You can check a directory element by clue or by typing the specific path. If you choose to go with the past, separate elemets with '/' character.</param>
        /// <param name="physical">Set it true if you want to check the actual directory or file. Set it false if you want to check if it is in the Element Library.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo Check(string clue, bool physical)
        {
            if (baseSet)
            {
                //Variables
                DirectoryElement element = GetDirectory(clue);
                string path = null;
                if (element != null)
                    path = element.path;

                if (path != null)
                {
                    if (!element.isFile)
                    {
                        if (!physical)
                        {
                            if (element != null)
                                return DirectoryOperationInfo.Success;
                            return DirectoryOperationInfo.MissingElement;
                        }
                        else
                        {
                            try
                            {
                                if (Directory.Exists(path))
                                    return DirectoryOperationInfo.Success;
                                return DirectoryOperationInfo.MissingElement;
                            }
                            catch { return DirectoryOperationInfo.MissingElement; }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!physical)
                            {
                                if (element != null)
                                    return DirectoryOperationInfo.Success;
                                return DirectoryOperationInfo.MissingElement;
                            }
                            else
                            {
                                if (File.Exists(path))
                                    return DirectoryOperationInfo.Success;
                                return DirectoryOperationInfo.MissingElement;
                            }
                        }
                        catch { return DirectoryOperationInfo.MissingElement; }
                    }
                }
                return DirectoryOperationInfo.MissingElement;
            }
            return DirectoryOperationInfo.NotSet;
        }

        /// <summary>
        /// Gets the element to a directory or file based on the 'clue'.
        /// </summary>
        /// <param name="clue">The clue can be the filename, a piece of the path, the nickname of the desired path or the entire path itself. Make sure to give a specific clue!</param>
        /// <returns>Returns the library element that has 'clue' as its property. Returns null if it can't find anything.</returns>
        public static DirectoryElement GetDirectory(string clue)
        {
            foreach (DirectoryElement current in elementLibrary)
            {
                if (current.nickname == clue || current.path.Contains(clue) || current.userPath == clue || current.path == clue)
                {
                    return current;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the whole library as it is stored.
        /// </summary>
        /// <returns>Returns a separated string with each library element in it.</returns>
        public static string GetStructure()
        {
            string structure = "";
            foreach (DirectoryElement current in elementLibrary)
            {
                structure += "NICK: " + current.nickname + "\n" + "PATH: " + current.path + "\n" + "USPATH: " + current.userPath + "\n" + "DIRPATH: " + current.dirPath + "\n\n\n";
            }
            return structure;
        }

        /// <summary>
        /// Sets the given nickname to an element.
        /// </summary>
        /// <param name="clue">The clue for the directory or file.</param>
        /// <param name="nickname">The nickname to set.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo SetNickname(string clue, string nickname)
        {
            DirectoryElement element = GetDirectory(clue);

            if (element == null)
                return DirectoryOperationInfo.MissingElement;

            element.nickname = nickname;
            return DirectoryOperationInfo.Success;
        }

        /// <summary>
        /// Sends every element in library into 'action'.
        /// </summary>
        /// <param name="action">The method you want to use each element for. Needs to take only DirectoryElement as parameter. Optional parameters are allowed as well.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo ForLibrary(Foreach action)
        {
            if (baseSet)
            {
                foreach(DirectoryElement current in elementLibrary) { action(current); }
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;
        }
        #endregion

        #region System Management
        /// <summary>
        /// Resets all the files and directories. Thus it returns them into their original state, erasing all existing data.
        /// </summary>
        /// <param name="clue">You can select a specific directory element by giving a clue about it.</param>
        public static void ResetStructure(string clue = null)
        {
            //If not specific
            if (clue == null)
            {
                reset = true;
                RepairStructure();
                reset = false;
            }
            //If specific dir
            else
            {
                DirectoryElement element = GetDirectory(clue);
                string path = null;
                if (element != null)
                    path = element.path;

                if (path != null)
                {
                    if (!element.isFile)
                    {
                        foreach (DirectoryElement current in elementLibrary)
                        {
                            if (current.isFile && current.dirPath == path)
                                File.Create(current.path).Dispose();
                            else if (!current.isFile && current.dirPath == path)
                                ResetStructure(current.userPath);
                        }
                    }
                    else
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                        File.Create(path).Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Searches for missing directories and files. Recreates the missing elements, while leaves the existing ones untouched.
        /// </summary>
        /// <returns></returns>
        public static DirectoryOperationInfo RepairStructure()
        {
            bool corrupted = false;

            if (baseSet)
            {
                foreach (DirectoryElement current in elementLibrary)
                {
                    if (!current.isFile)
                    {
                        if (Directory.Exists(current.path))
                            continue;
                        Directory.CreateDirectory(current.path);
                        corrupted = true;
                        continue;
                    }
                    else
                    {
                        if (Directory.Exists(current.dirPath))
                        {
                            if (File.Exists(current.path))
                            {
                                if (reset) { File.Delete(current.path); File.Create(current.path).Dispose(); }
                                continue;
                            }
                            File.Create(current.path).Dispose();
                            corrupted = true;
                            continue;
                        }
                        else
                        {
                            Directory.CreateDirectory(current.dirPath);
                            File.Create(current.path).Dispose();
                            corrupted = true;
                            continue;
                        }
                    }
                }
                if (corrupted)
                    return DirectoryOperationInfo.DirectoryCorrupted;
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;
        }

        /// <summary>
        /// Whenever a new file or directory is added, this method is called and it checks the path to that new element. On the path, it adds every element to the library to have a reference for that.
        /// </summary>
        /// <param name="givenPath"></param>
        private static void LibraryFill(string givenPath)
        {
            string[] elements = givenPath.Split('/');
            string path = rootDirectory;
            string dirPath = path;
            bool exists = false;

            for (int i = 0; i < elements.Length - 1; i++)
            {
                path = Path.Combine(path, elements[i]);
                foreach (DirectoryElement current in elementLibrary) { if (current.path == path) { exists = true; break; } }
                if (!exists)
                {
                    elementLibrary.Add(new DirectoryElement(path, false, dirPath, null, givenPath));
                }                    
                exists = false;
                dirPath = path;
            }
        }
        #endregion
    }
}