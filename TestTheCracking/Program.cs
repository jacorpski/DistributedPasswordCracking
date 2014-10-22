using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestTheCracking
{
    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            Buffer sharedBuffer1 = new Buffer();
            Buffer sharedBuffer2 = new Buffer();

            TaskFactory f = new TaskFactory();

            var stage1 = f.StartNew(() => Stage1Buffer(sharedBuffer1));
            var stage2 = f.StartNew(() => Stage2Buffer(sharedBuffer1, sharedBuffer2));

            Task.WaitAll(stage1, stage2);

            int count = sharedBuffer2.getCount();

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(sharedBuffer2.Take());
            }

            Console.ReadLine();
        }

        static void Stage1Buffer(Buffer sharedBufferIn)
        {
            for (int i = 0; i < 20; i++)
            {
                int randomNumber = random.Next();



                if (sharedBufferIn.Put(randomNumber))
                {
                    Console.WriteLine("Stage 1: " + randomNumber);
                }
                else
                {
                    Console.WriteLine("Stage 1: Buffer is full");
                }
            }

            
        }

        static void Stage2Buffer(Buffer sharedBufferOut, Buffer sharedBufferIn)
        {
            for (int i = 0; i < 20; i++)
            {
                int buffer = sharedBufferOut.Take();

                if (buffer != -1)
                {
                    buffer = buffer + 10;

                    Console.WriteLine("Stage 2: " + buffer);

                    sharedBufferIn.Put(buffer);
                }
                else
                {
                    Console.WriteLine("Stage 2: Buffer empty");
                }
            }
        }
    }
}
