using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Io.Rz.FlywheelComponents.Util;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Io.Rz.FlywheelComponents.ChunkingTests
{
    /*
    * Licensed to the Apache Software Foundation (ASF) under one or more
    * contributor license agreements.  See the NOTICE file distributed with
    * this work for additional information regarding copyright ownership.
    * The ASF licenses this file to You under the Apache License, Version 2.0
    * (the "License"); you may not use this file except in compliance with
    * the License.  You may obtain a copy of the License at
    * 
    *      http://www.apache.org/licenses/LICENSE-2.0
    * 
    * Unless required by applicable law or agreed to in writing, software
    * distributed under the License is distributed on an "AS IS" BASIS,
    * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    * See the License for the specific language governing permissions and
    * limitations under the License.
    */

    [TestClass]
    public class BuzHashTests
    {
        [TestMethod]
        public void TestBuzHashBasic()
        {
            BuzHashFunction h = new BuzHashFunction();
            Assert.AreNotEqual(h.Hash("foo"), h.Hash("bar"));
            Assert.AreNotEqual(h.Hash("foo"), h.Hash("oof"));
            Assert.AreNotEqual(h.Hash("1234567890"), h.Hash("1134567890"));

            Assert.AreEqual((uint)604458852, h.Hash("aaabbb12334567rp6ejflskxnjclzjflaksjfdlaksjlaksjdasd"));


        }

        [TestMethod]
        public void TestBuzHashStream()
        {
            var hash = new BuzHashFunction();
            var foo = new MemoryStream(Encoding.UTF8.GetBytes("foo"));
            var oof = new MemoryStream(Encoding.UTF8.GetBytes("oof"));
            var bar = new MemoryStream(Encoding.UTF8.GetBytes("bar"));
            var s1 = new MemoryStream(Encoding.UTF8.GetBytes("1234567890"));
            var s2 = new MemoryStream(Encoding.UTF8.GetBytes("1134567890"));


            Assert.AreNotEqual(hash.Hash(foo,32), hash.Hash(bar,32));
            foo.Seek(0, SeekOrigin.Begin);
            Assert.AreNotEqual(hash.Hash(foo,32), hash.Hash(oof,32));
            Assert.AreNotEqual(hash.Hash(s1,32), hash.Hash(s2,32));
        }


        [TestMethod]
        public void TestBuzHashSlide()
        {
            BuzHashFunction h = new BuzHashFunction();
            byte[] bytes = { 2, 2, 2, 5, 7, 9 };
            uint hash = h.Hash(bytes, 3, 3);

            uint newHash = h.Hash(bytes, 0, 3);

            for (int i = 1; i < bytes.Length - 2; i++)
            {
                newHash = h.UpdateHash(newHash, bytes[i + 2], bytes[i - 1], 3);
            }
            Assert.AreEqual(hash, newHash);

        }

        [TestMethod]
        public void TestBuzHashSlideQueue()
        {
            int windowSize = 4;
            Queue<byte> queue = new Queue<byte>(windowSize);
            BuzHashFunction h = new BuzHashFunction();
            byte[] bytes = { 2, 2, 2, 5, 7, 9,10 };
            uint hash = h.Hash(bytes, bytes.Length - windowSize, windowSize);

            uint newHash = h.Hash(bytes, 0, windowSize);
            for (int i = 0; i < windowSize; i++)
                queue.Enqueue(bytes[i]);

            for (int i = windowSize; i < bytes.Length; i++)
            {
                newHash = h.UpdateHash(newHash, bytes[i], queue.Dequeue(), windowSize);
                queue.Enqueue(bytes[i]);
            }
            Assert.AreEqual(hash, newHash);

        }

        [TestMethod]
        public void TestBuzHashSlideQueueBigWindow()
        {
            Queue<byte> queue = new Queue<byte>(32);
            BuzHashFunction h = new BuzHashFunction();
            byte[] bytes = Encoding.UTF8.GetBytes("aaabbb12334567rp6ejflskxnjclzjflaksjfdlaksjlaksjdasd");
            uint hash = h.Hash(bytes, bytes.Length-32, 32);


            uint newHash = h.Hash(bytes, 0, 32);
            for (int i = 0; i < 32; i++)
            {
                queue.Enqueue(bytes[i]);
            }

            for (int i = 32; i < bytes.Length ; i++)
            {
                newHash = h.UpdateHash(newHash, bytes[i], queue.Dequeue(), 32);
                queue.Enqueue(bytes[i]);
            }
            Assert.AreEqual(hash, newHash);

        }
    }
}
