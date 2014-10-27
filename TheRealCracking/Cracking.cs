using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheRealCracking
{
    public class Cracking
    {
        // Contain the hash algorithm we choose to use
        private readonly HashAlgorithm _messageDigest;

        // All the buffers we need
        private Buffer BufferOfStrings          = new Buffer(10);
        private Buffer BufferOfEncryptedStrings = new Buffer(10);
        private Buffer BufferOfMatches          = new Buffer(10);

        // Hold the dictionary file
        private FileStream dictionaryFile;

        // Constructor - set the hash algorithm to use
        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
        }
        

        public void RunCracking()
        {
            ReadDictionary();

            TaskFactory f = new TaskFactory();

            // The stages the program are going to use
            var ReadAllStringsStage = f.StartNew(() => ReadAllStrings(dictionaryFile, BufferOfStrings));

            // We need to wait on all stages before we can continue
            Task.WaitAll(ReadAllStringsStage);

            // Close the dictionary file
            dictionaryFile.Close();
        }

        private void ReadDictionary()
        {
            // Open and read the dictionary
            dictionaryFile = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read);
        }

        private void ReadAllStrings(FileStream dictionary, Buffer sharedBuffer)
        {
            using (StreamReader temp = new StreamReader(dictionary))
            {
                // If we hadn't hit the end of the file, then continue
                while (!temp.EndOfStream)
                {
                    // Temporary place for the string
                    String put = temp.ReadLine();

                    if (sharedBuffer.Put(put))
                    {
                        Console.WriteLine("Added " + put + " to BufferOfStrings");
                    }
                }
            }
        }
    }
}
