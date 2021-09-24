using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DirectoryManagement
{
    public static class FileManager
    {
        #region Properties
        //General
        private static int key = 0;
        private static bool set = false;
        private static bool safety = false;

        //Converter
        private static BaseConverter converter;

        /// <summary>
        /// Returns the state of the FileManager.
        /// </summary>
        public static bool active { get { return set; } }
        /// <summary>
        /// Returns the encryption key you specified.
        /// </summary>
        public static int encryptionKey { get { return key; } }
        /// <summary>
        /// Returns the if the FileManager is in safe mode or not.
        /// </summary>
        public static bool inSafeMode { get { return safety; } }
        /// <summary>
        /// Returns the instance of the converter that the FileManager uses.
        /// </summary>
        public static BaseConverter usedConverter { get { return converter; } }
        #endregion

        #region Basic Functions
        /// <summary>
        /// Sets up the FileManager to use. Call this method first. Specify an encryption key for the files. This does not make the files encrypted, but if you encrypt them, this key will be used.
        /// </summary>
        /// <param name="_encryptionKey">The number that will be used to encrypt files. You can specify a number as huge as you want (32-bit integer), but the bigger the number the bigger the filesize is (and the memory usage on decryption/encryption). Using 0 will lead to primitive encryption. It is suggested to use numbers between 3 and 50.</param>
        /// <param name="_converter">You need to pass an instance of a converter class that inherits from 'BaseConverter'. Override its methods to create your own converter.</param>
        /// <param name="_safety">Set this false if you will want to change the encryption key later. It is not recommended.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo SetupFileManager(int _encryptionKey, BaseConverter _converter, bool _safety = true)
        {
            if (DirectoryStructure.active)
            {
                if (!set || !safety)
                {
                    key = _encryptionKey;
                    if (key == 0)
                        key = 1;
                    safety = _safety;
                    converter = _converter;
                    set = true;
                    return DirectoryOperationInfo.Success;
                }
                return DirectoryOperationInfo.AlreadySet;
            }
            return DirectoryOperationInfo.NotSet;
        }

        /// <summary>
        /// Saves an object to a file with the given parameters. It overwrites the first line of the file by default.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="_object">The object to save (type must match with T).</param>
        /// <param name="clue">The clue for the desired file.</param>
        /// <param name="extendFile">If you want to save the object as a new line in the file, set this true. If you want to replace one, set this to false.</param>
        /// <param name="line">If you want to overwrite a specific line, set this as line index.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo Save<T>(T _object, string clue, bool extendFile = false, int line = 0)
        {
            if (DirectoryStructure.active && set)
            {
                DirectoryElement element = DirectoryStructure.GetDirectory(clue);

                if (element == null)
                    return DirectoryOperationInfo.MissingElement;
                if (!element.isFile)
                    return DirectoryOperationInfo.InvalidElementType;

                //Serialize the object and encrypt if desired
                string data = converter.Serialize(_object);
                string[] file = File.ReadAllLines(element.path);

                //If we want to add a new line to the file
                if (extendFile)
                {
                    //Add data to the file lines
                    List<string> fileList = file.ToList();
                    fileList.Add(data);
                    file = fileList.ToArray();
                }
                else
                {
                    try
                    {
                        if (file.Length == 0)
                            file = new string[1];
                        file[line] = data;
                    }
                    catch (IndexOutOfRangeException) { return DirectoryOperationInfo.FileIndexOverflow; }
                }
                File.WriteAllLines(element.path, file, Encoding.UTF8);
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;
        }

        /// <summary>
        /// Creates an object from a line of a file.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="clue">The clue for the file in DirectoryStructure library.</param>
        /// <param name="line">The line of the file to be read. Zero by default.</param>
        /// <returns>Tries to return an object based on T. If something fails (line is out of range or file cannot be read) it returns a n object default (null).</returns>
        public static T Get<T>(string clue, int line = 0)
        {
            if (DirectoryStructure.active && set)
            {
                DirectoryElement element = DirectoryStructure.GetDirectory(clue);

                if (element == null)
                    return default(T);
                if (!element.isFile)
                    return default(T);

                //Get the file
                string[] file = File.ReadAllLines(element.path);
                T _object;
                //Convert into object
                try { _object = converter.Deserialize<T>(file[line]); }
                catch { return default(T); }

                return _object;
            }
            return default(T);
        }

        /// <summary>
        /// Encrypts or decrypts a file with the given key.
        /// </summary>
        /// <param name="clue">The clue for the desired DirectoryStructure file.</param>
        /// <param name="encrypt">True to encrypt, false to decrypt.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo EncryptFile(string clue, bool encrypt)
        {
            if (DirectoryStructure.active && set)
            {
                DirectoryElement element = DirectoryStructure.GetDirectory(clue);

                if (element == null)
                    return DirectoryOperationInfo.MissingElement;
                if (!element.isFile)
                    return DirectoryOperationInfo.InvalidElementType;

                //Do the encryption / decription
                string[] file = File.ReadAllLines(element.path, Encoding.UTF8);
                for (int i = 0; i < file.Length; i++) { Encrypt(ref file[i], encrypt); }

                //Write back to file
                File.WriteAllLines(element.path, file, Encoding.UTF8);

                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;
        }

        /// <summary>
        /// Encrypts or decrypts all files.
        /// </summary>
        /// <param name="encrypt">True to encrypt, false to decrypt.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo EncryptFiles(bool encrypt)
        {
            if (DirectoryStructure.active && set)
            {
                DirectoryStructure.ForLibrary(Each);
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;

            void Each(DirectoryElement element)
            {
                if (element.isFile && encrypt)
                    EncryptFile(element.path, encrypt);
                if (element.isFile && !encrypt)
                    EncryptFile(element.path, encrypt);
            }
        }

        /// <summary>
        /// Checks if a file can be read or not. If not, it might be because it is encrypted.
        /// </summary>
        /// <typeparam name="T">You need to specify an object you want your file to be tested. The funtction will try and create an instance from the file.</typeparam>
        /// <param name="clue">The clue for the file.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo CheckFileReadability<T>(string clue)
        {
            if (DirectoryStructure.active)
            {
                DirectoryElement element = DirectoryStructure.GetDirectory(clue);

                if (element == null)
                    return DirectoryOperationInfo.MissingElement;
                if (!element.isFile)
                    return DirectoryOperationInfo.InvalidElementType;

                string[] file = File.ReadAllLines(element.path);
                try { T _object = converter.Deserialize<T>(file[0]); }
                catch { return DirectoryOperationInfo.FileUnreadable; }
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;
        }
        #endregion

        #region System Functions
        /// <summary>
        /// Encrypts or decrypts a data string.
        /// </summary>
        /// <param name="data">The data to be crypted.</param>
        /// <param name="encrypt">True to encrypt, false to decrypt.</param>
        public static void Encrypt(ref string data, bool encrypt)
        {
            if (encrypt)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                for (int i = 0; i < bytes.Length; i++) { BitPush(ref bytes[i], encrypt); }

                //into int an string
                int[] intBytes = new int[bytes.Length];
                string result = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    //Int
                    int newValue = bytes[i] * key;
                    intBytes[i] = newValue;
                    //String
                    if (i == bytes.Length - 1)
                        result += newValue;
                    else
                        result += newValue + "?";
                }
                data = result;
                return;
            }
            else
            {
                try
                {
                    string[] strings = data.Split('?');
                    int[] ints = Array.ConvertAll(strings, int.Parse);
                    byte[] bytes = new byte[ints.Length];
                    for (int i = 0; i < ints.Length; i++) { bytes[i] = Convert.ToByte(ints[i] / key); }
                    for (int i = 0; i < bytes.Length; i++) { BitPush(ref bytes[i], encrypt); }
                    data = Encoding.UTF8.GetString(bytes);
                }
                catch { }
            }
        }

        private static void BitPush(ref byte data, bool encrypt)
        {
            string dataByte = Convert.ToString(data, 2).PadLeft(8, '0');
            char[] result = new char[8];
            int validKey = key % 8;

            //Encrypt
            if (encrypt)
            {
                for (int i = 0; i < 8; i++)
                {
                    int position = i + validKey;
                    if (position >= 8) { position -= 8; }
                    result[position] = dataByte[i];
                }
            }
            //Decrypt
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    int position = i - validKey;
                    if (position < 0) { position += 8; }
                    result[position] = dataByte[i];
                }
            }

            //Return
            data = Convert.ToByte(new string(result), 2);
        }

        /// <summary>
        /// Reads a line of a file. If encrypted, it returns it decrypted. It does not write into file.
        /// </summary>
        /// <param name="clue">The clue for the Directory Element.</param>
        /// <param name="line">The line of the file (as index).</param>
        /// <param name="encrypted">True if the file is encrypted, false if it is not.</param>
        /// <returns></returns>
        public static string ReadLine(string clue, int line, bool encrypted)
        {
            if (!DirectoryStructure.active || !set)
                return null;

            DirectoryElement element = DirectoryStructure.GetDirectory(clue);
            if (element == null)
                return null;
            if (!element.isFile)
                return null;

            string[] file = File.ReadAllLines(element.path);
            string result = "";
            try { result = file[line]; }
            catch { return null; }

            if (encrypted)
                Encrypt(ref result, false);
            return result;
        }

        /// <summary>
        /// Returns the lines of the file as a string array. Returns null if something went wrong.
        /// </summary>
        /// <param name="clue">The clue for the DirectoryElement.</param>
        /// <param name="decrypt">True to decrypt the file before reading and encrypt after.</param>
        /// <returns></returns>
        public static string[] GetFile(string clue, bool decrypt)
        {
            if (!DirectoryStructure.active || !set)
                return null;

            DirectoryElement element = DirectoryStructure.GetDirectory(clue);
            if (element == null)
                return null;
            if (!element.isFile)
                return null;

            if (decrypt)
                EncryptFile(clue, false);
            string[] file = File.ReadAllLines(element.path);
            if (decrypt)
                EncryptFile(clue, true);

            return file;
        }

        /// <summary>
        /// Tries to create an instance of T from 'data'.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="data">The string to get the object from.</param>
        /// <returns>Returns an object from the string. If the operation cannot be terminated, it returns object default (null).</returns>
        public static T GetObject<T>(string data)
        {
            T _object;
            try { _object = converter.Deserialize<T>(data); }
            catch { return default(T); }

            return _object;
        }

        /// <summary>
        /// Reset the encryption key. Not suggested for normal usage! Only for advanced users. Cannot be used when safety is enabled.
        /// </summary>
        /// <param name="key">The new encryption key.</param>
        /// <returns></returns>
        public static DirectoryOperationInfo ChangeEncryptionKey(int newKey)
        {
            if (DirectoryStructure.active && set)
            {
                if (safety)
                    return DirectoryOperationInfo.SafetyEnabled;

                key = newKey;
                return DirectoryOperationInfo.Success;
            }
            return DirectoryOperationInfo.NotSet;
        }
        #endregion
    }
}