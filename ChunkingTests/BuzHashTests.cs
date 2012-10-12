using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Io.Rz.FlywheelComponents.Chunking;

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

            Assert.AreEqual(0xbee30996, h.Hash("aaabbb12334567rp6ejflskxnjclzjflaksjfdlaksjlaksjdasd"));

           
        }


        [TestMethod]
        public void TestBuzHashSlide()
        {
            BuzHashFunction h = new BuzHashFunction();
            byte[] bytes = { 2, 2, 2, 5, 7, 9 };
            uint hash = h.NonRollingHash(bytes, 3, 3);

            uint newHash = h.Hash(bytes, 0, 3);

            for (int i = 1; i < bytes.Length - 2; i++)
            {
                newHash = h.UpdateHash(newHash, bytes[i + 2], bytes[i - 1], 3);
            }
            Assert.AreEqual(hash, newHash);

        }


    }
}
