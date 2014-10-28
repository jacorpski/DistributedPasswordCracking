﻿using System;
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
        // Contain the hash algorithm we choose to use
        private readonly HashAlgorithm _messageDigest;

        private static readonly Converter<char, byte> Converter = CharToByte;

        // All the buffers we need
        private Buffer BufferOfStrings          = new Buffer(1000000);
        private Buffer BufferOfEncryptedStrings = new Buffer(1000000);
        private Buffer BufferOfMatches          = new Buffer(1000000);

        // Hold the dictionary file
        private FileStream dictionaryFile;

        private List<String> passwordList = new List<string>(); 

        // Constructor - set the hash algorithm to use
        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
        }
        

        public void RunCracking()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ReadDictionary();
            ReadPasswords();

            TaskFactory f = new TaskFactory();

            // The stages the program are going to use
            var ReadAllStringsStage = f.StartNew(() => ReadAllStrings(dictionaryFile, BufferOfStrings));
            var EncryptedStringsStage = f.StartNew(() => EncryptedStrings(BufferOfStrings, BufferOfEncryptedStrings));
            var CompareStringsStage = f.StartNew(() => CompareStrings(BufferOfEncryptedStrings, BufferOfMatches));

            // We need to wait on all stages before we can continue
            Task.WaitAll(ReadAllStringsStage, EncryptedStringsStage, CompareStringsStage);
            
            // Close the dictionary file
            dictionaryFile.Close();

            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        }

        private void ReadDictionary()
        {
            // Open and read the dictionary
            dictionaryFile = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read);
        }

        private void ReadPasswords()
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
        }

        private void ReadAllStrings(FileStream dictionary, Buffer sharedBuffer)
        {
            int i = 0;
            using (StreamReader temp = new StreamReader(dictionary))
            {
                // If we hadn't hit the end of the file, then continue
                while (!temp.EndOfStream)
                {
                    // Temporary place for the string
                    String put = temp.ReadLine();

                    if (sharedBuffer.Put(put))
                    {
                        //Console.WriteLine("Added " + put + " to BufferOfStrings");
                    }

                    i++;
                }
            }

            Console.WriteLine("Lines in the file: "+ i);

            //Console.WriteLine("BufferOfStrings: "+ sharedBuffer.getCount());

            sharedBuffer.MarkCompleted();
        }

        private void EncryptedStrings(Buffer sharedBufferOut, Buffer sharedBufferIn)
        {
            int i = 0;
            while (!sharedBufferOut.IsCompleted() || !sharedBufferOut.IsEmpty())
            {
                string temp = sharedBufferOut.Take();

                if (temp != "")
                {
                    byte[] tempByted = Array.ConvertAll(temp.ToCharArray(), GetConverter());
                    byte[] tempBytedEncrypted = _messageDigest.ComputeHash(tempByted);
                    string tempBytedEncryptedStringed = Convert.ToBase64String(tempBytedEncrypted);

                    sharedBufferIn.Put(temp + ":" + tempBytedEncryptedStringed);

                    i++;
                }
            }

            Console.WriteLine("EncryptedStrings Rounds: "+ i);

            sharedBufferIn.MarkCompleted();
            
        }

        private void CompareStrings(Buffer sharedBufferOut, Buffer sharedBufferIn)
        {
            int i = 0;
            while (!sharedBufferOut.IsCompleted() || !sharedBufferOut.IsEmpty())
            {
                string temp = sharedBufferOut.Take();

                if (temp != "")
                {
                    string[] tempSplit = temp.Split(':');

                    Console.WriteLine(tempSplit[0] +" er i krypteret form "+ tempSplit[1]);

                    foreach (String password in passwordList)
                    {
                        string[] passwordSplit = password.Split(':');

                        if (tempSplit[1].Equals(passwordSplit[1]))
                        {
                            Console.WriteLine(passwordSplit[0] +" password is "+ tempSplit[0]);
                        }
                    }







                    sharedBufferIn.Put(temp);

                    i++;
                }
            }

            Console.WriteLine("CompareStrings Rounds: " + i);
            Console.WriteLine("CompareStrings: " + sharedBufferIn.getCount());
            Console.WriteLine("");

            Console.WriteLine("End");
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
