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
        private readonly HashAlgorithm _messageDigest;

        private Buffer BufferOfStrings          = new Buffer(10);
        private Buffer BufferOfEncryptedStrings = new Buffer(10);
        private Buffer BufferOfMatches          = new Buffer(10);

        private FileStream dictionaryFile;

        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
        }


        public void RunCracking()
        {
            ReadDictionary();

            TaskFactory f = new TaskFactory();

            var ReadAllStringsStage = f.StartNew(() => ReadAllStrings(dictionaryFile, BufferOfStrings));

            Task.WaitAll(ReadAllStringsStage);

            dictionaryFile.Close();
        }

        private void ReadDictionary()
        {
            dictionaryFile = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read);
        }

        private void ReadAllStrings(FileStream dictionary, Buffer sharedBuffer)
        {
            using (StreamReader temp = new StreamReader(dictionary))
            {
                while (!temp.EndOfStream)
                {
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
