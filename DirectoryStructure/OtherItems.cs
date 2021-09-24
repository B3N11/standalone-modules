namespace DirectoryManagement
{
    public class DirectoryElement
    {
        public string path;
        public string dirPath;
        public string userPath;
        public bool isFile;

        public string nickname;

        public DirectoryElement(string _path, bool _isFile, string _dirPath = null, string _nickname = null, string _userPath = null)
        {
            path = _path;
            isFile = _isFile;
            dirPath = _dirPath;
            nickname = _nickname;
            userPath = _userPath;
        }
    }

    public enum DirectoryOperationInfo
    {
        /// <summary>
        /// Operation was successful.
        /// </summary>
        Success,
        /// <summary>
        /// An unhandled exception occured.
        /// </summary>
        Failure,
        /// <summary>
        /// The desired directory or file does not exist.
        /// </summary>
        MissingElement,
        /// <summary>
        /// The desired directory or file already exists.
        /// </summary>
        ElementAlreadyExists,
        /// <summary>
        /// Some of the files or directories were deleted and had to be recreated.
        /// </summary>
        DirectoryCorrupted,
        /// <summary>
        /// The root directory or the FileManager is not set yet. Call 'SetupStructure' or 'SetupFileManager' first.
        /// </summary>
        NotSet,
        /// <summary>
        /// The structure or the file manager has already been configured.
        /// </summary>
        AlreadySet,

        /// <summary>
        /// If the selected element's type does not fit for the desired operation (eg.: you specified a directory instead of a file)
        /// </summary>
        InvalidElementType,
        /// <summary>
        /// The file has no line on the given index.
        /// </summary>
        FileIndexOverflow,
        /// <summary>
        /// The desired file is encrypted and/or cannot be read.
        /// </summary>
        FileUnreadable,
        /// <summary>
        /// The safe mode has been enabled on setup.
        /// </summary>
        SafetyEnabled
    }

    public class BaseConverter
    {
        public virtual string Serialize<T>(T _object)
        {
            throw new System.NotImplementedException();
        }

        public virtual T Deserialize<T>(string data)
        {
            throw new System.NotImplementedException();
        }
    }
}
