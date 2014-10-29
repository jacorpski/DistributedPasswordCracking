using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TheRealCracking
{
    public class Cracking
    {

        private static readonly Converter<char, byte> Converter = CharToByte;

        // All the buffers we need
        private Buffer BufferOfStrings          = new Buffer(51000);
        private Buffer BufferOfEncryptedStrings = new Buffer(10000000);
        private Buffer BufferOfMatches          = new Buffer(10);

        // Hold the dictionary file
        private FileStream dictionaryFile;

        private int _encryptedStringsTasks = 0;
        private int _encryptedStringsCompleted = 0;

        private int _compareStringsTasks = 0;
        private int _compareStringsCompleted = 0;

        private List<Task> tasks = new List<Task>(); 

        // Constructor - set the hash algorithm to use
        public Cracking()
        {
            
        }
        

        public void RunCracking()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ReadDictionary();
            

            TaskFactory f = new TaskFactory();

            // The stages the program are going to use
            tasks.Add(f.StartNew(() => ReadAllStrings(dictionaryFile, BufferOfStrings)));
            tasks.Add(f.StartNew(() => EncryptedStrings(BufferOfStrings, BufferOfEncryptedStrings)));
            tasks.Add(f.StartNew(() => EncryptedStrings(BufferOfStrings, BufferOfEncryptedStrings)));
            tasks.Add(f.StartNew(() => EncryptedStrings(BufferOfStrings, BufferOfEncryptedStrings)));
            tasks.Add(f.StartNew(() => CompareStrings(BufferOfEncryptedStrings, BufferOfMatches)));
            tasks.Add(f.StartNew(() => CompareStrings(BufferOfEncryptedStrings, BufferOfMatches)));
            tasks.Add(f.StartNew(() => PrintMatches(BufferOfMatches)));


            // We need to wait on all stages before we can continue
            Task.WaitAll(tasks.ToArray());
            
            // Close the dictionary file
            dictionaryFile.Close();

            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        }

        private void ReadDictionary()
        {
            // Open and read the dictionary
            dictionaryFile = new FileStream("webster-dictionary_original.txt", FileMode.Open, FileAccess.Read);
        }

        /*private void ReadPasswords()
        {
            using (FileStream fs = new FileStream("passwords.txt", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader passwords = new StreamReader(fs))
                {
                    while (!passwords.EndOfStream)
                    {
                        passwordList.Add(passwords.ReadLine());
                    }
                }
            }
        }*/

        private void ReadAllStrings(FileStream dictionary, Buffer sharedBuffer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int i = 0;
            using (StreamReader temp = new StreamReader(dictionary))
            {
                // If we hadn't hit the end of the file, then continue
                while (!temp.EndOfStream)
                {
                    // Temporary place for the string
                    String put = temp.ReadLine();

                    sharedBuffer.Put(put);

                    i++;
                }
            }

            Console.WriteLine("Lines in the file: "+ i);

            sharedBuffer.MarkCompleted();

            stopwatch.Stop();
            Console.WriteLine("ReadAllTheStrings time: {0}", stopwatch.Elapsed);
        }

        private void EncryptedStrings(Buffer sharedBufferOut, Buffer sharedBufferIn)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _encryptedStringsTasks++;

            HashAlgorithm _messageDigest;
            _messageDigest = new SHA1CryptoServiceProvider();

            int i = 0;
            while (!sharedBufferOut.IsCompleted() || !sharedBufferOut.IsEmpty())
            {
                string temp = sharedBufferOut.Take();

                if (temp != "")
                {
                    List<String> tempVariations = MadePasswordVariations(temp);

                    foreach (string password in tempVariations)
                    {
                        byte[] passwordByted = Array.ConvertAll(password.ToCharArray(), GetConverter());
                        byte[] passwordBytedEncrypted = _messageDigest.ComputeHash(passwordByted);
                        string passwordBytedEncryptedStringed = Convert.ToBase64String(passwordBytedEncrypted);

                        sharedBufferIn.Put(password + ":" + passwordBytedEncryptedStringed);

                        i++;
                    }
                }
            }

            _encryptedStringsCompleted++;

            if (_encryptedStringsCompleted == _encryptedStringsTasks)
            {
                sharedBufferIn.MarkCompleted();
            }

            stopwatch.Stop();
            Console.WriteLine("EncryptedStrings time({0}): {1}", _encryptedStringsCompleted, stopwatch.Elapsed);
        }

        private void CompareStrings(Buffer sharedBufferOut, Buffer sharedBufferIn)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _compareStringsTasks++;

            List<String> passwordList = new List<string>();

            using (FileStream fs = new FileStream("passwords.txt", FileMode.Open, FileAccess.Read))
            {
                using (StreamReader passwords = new StreamReader(fs))
                {
                    while (!passwords.EndOfStream)
                    {
                        passwordList.Add(passwords.ReadLine());
                    }
                }
            }

            int i = 0;
            while (!sharedBufferOut.IsCompleted() || !sharedBufferOut.IsEmpty())
            {
                string temp = sharedBufferOut.Take();

                if (temp != "")
                {
                    string[] tempSplit = temp.Split(':');

                    List<String> tempRemove = new List<string>();

                    foreach (String password in passwordList)
                    {
                        string[] passwordSplit = password.Split(':');

                        if (tempSplit[1].Equals(passwordSplit[1]))
                        {
                            sharedBufferIn.Put(passwordSplit[0] + ":" + tempSplit[0]);

                            tempRemove.Add(password);
                        }
                    }

                    if (tempRemove.Count > 0)
                    {
                        foreach (string removePassword in tempRemove)
                        {
                            passwordList.Remove(removePassword);
                        }

                        tempRemove.Clear();
                    }


                    i++;
                }
            }

            _compareStringsCompleted++;

            if (_compareStringsCompleted == _compareStringsTasks)
            {
                sharedBufferIn.MarkCompleted();
            }

            stopwatch.Stop();
            Console.WriteLine("CompareStrings time({0}): {1}", _compareStringsCompleted, stopwatch.Elapsed);
        }

        private void PrintMatches(Buffer sharedBuffer)
        {
            while (!sharedBuffer.IsCompleted() || !sharedBuffer.IsEmpty())
            {
                string temp = sharedBuffer.Take();

                if (temp != "")
                {
                    Console.WriteLine("MATCHES: " + temp);
                }
            }
        }

        private List<String> MadePasswordVariations(string password)
        {
            List<String> temp = new List<string>();

            temp.Add(password);

            String passwordUpperCase = password.ToUpper();
            temp.Add(passwordUpperCase);

            String passwordCapitalized = StringUtilities.Capitalize(password);
            temp.Add(passwordCapitalized);

            String passwordReverse = StringUtilities.Reverse(password);
            temp.Add(passwordReverse);

            for (int i = 0; i < 100; i++)
            {
                String passwordEndDigit = password + i;
                temp.Add(passwordEndDigit);
            }

            for (int i = 0; i < 100; i++)
            {
                String passwordStartDigit = i + password;
                temp.Add(passwordStartDigit);
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    String passwordStartEndDigit = i + password + j;
                    temp.Add(passwordStartEndDigit);
                }
            }

            return temp;
        }

        public static Converter<char, byte> GetConverter()
        {
            return Converter;
        }

        private static byte CharToByte(char ch)
        {
            return Convert.ToByte(ch);
        }
    }
}
