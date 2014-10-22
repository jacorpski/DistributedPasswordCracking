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
        private ConcurrentQueue<int> sharedBuffer = new ConcurrentQueue<int>();


        public void Put(int value)
        {
            sharedBuffer.Enqueue(value);
        }

        public int Take()
        {
            int value;
            int outValue = -1;

            if (!IsEmpty())
            {
                if (sharedBuffer.TryDequeue(out value))
                {
                    outValue = value;
                }
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
