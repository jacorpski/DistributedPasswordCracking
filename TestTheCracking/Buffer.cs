using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTheCracking
{
    public class Buffer
    {
        private BlockingCollection<int> sharedBuffer = new BlockingCollection<int>(10);


        public bool Put(int value)
        {
            return sharedBuffer.TryAdd(value);
        }

        public int Take()
        {
            int value;
            int outValue = -1;

            
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
