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
        /// <summary>
        /// Initialize the property of the BlockingCollection
        /// </summary>
        private BlockingCollection<string> sharedBuffer;

        /// <summary>
        /// Initialize the BlockingCollection and set the capacity
        /// </summary>
        /// <param name="capacity">The capacity of the buffer</param>
        public Buffer(int capacity)
        {
            sharedBuffer = new BlockingCollection<string>(capacity);
        }

        /// <summary>
        /// Put the value in the BlockingCollection
        /// </summary>
        /// <param name="value">The string to put in</param>
        /// <returns>The boolean of the result</returns>
        public bool Put(string value)
        {
            return sharedBuffer.TryAdd(value, -1);
        }

        /// <summary>
        /// Take the value of the BlockingCollection and returns the value of it
        /// </summary>
        /// <returns>The value of the taken value of the BlockingCollection</returns>
        public string Take()
        {
            string value;
            string outValue = "";


            if (sharedBuffer.TryTake(out value, -1))
            {
                outValue = value;
            }

            return outValue;
        }

        /// <summary>
        /// Check if the BlockingCollection is empty
        /// </summary>
        /// <returns>If empty, true, else return false</returns>
        public bool IsEmpty()
        {
            return getCount() <= 0;
        }

        /// <summary>
        /// Get the number of entries in the BlockingCollection
        /// </summary>
        /// <returns>The number of entries in the BlockingCollection</returns>
        public int getCount()
        {
            return sharedBuffer.Count;
        }

        /// <summary>
        /// Mark the BlockingCollection done for adding.
        /// </summary>
        public void MarkCompleted()
        {
            sharedBuffer.CompleteAdding();
        }

        /// <summary>
        /// Check if the BlockingCollection is marked done.
        /// </summary>
        /// <returns>Returns true if the BlockingCollection is marked done, else false</returns>
        public bool IsCompleted()
        {
            return sharedBuffer.IsAddingCompleted;
        }
    }
}
