using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRealCracking
{
    public class Buffer
    {
        private BlockingCollection<string> sharedBuffer;

        public Buffer(int capacity)
        {
            sharedBuffer = new BlockingCollection<string>(capacity);
        }

        public bool Put(string value)
        {
            return sharedBuffer.TryAdd(value);
        }

        public string Take()
        {
            string value;
            string outValue = "";


            if (sharedBuffer.TryTake(out value))
            {
                outValue = value;
            }

            return outValue;
        }

        public bool IsEmpty()
        {
            return getCount() <= 0;
        }

        public int getCount()
        {
            return sharedBuffer.Count;
        }
    }
}
