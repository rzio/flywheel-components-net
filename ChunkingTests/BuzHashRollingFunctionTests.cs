using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Io.Rz.FlywheelComponents.Util;
using System.IO;
using System.Text;
using System.Collections.Generic;
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
    public class BuzHashRollingHasherTests
    {

        [TestMethod]
        public void TestBuzHashStreamWithSlide()
        {
            var stringBytes = Encoding.UTF8.GetBytes("aaabbb12334567rp6ejflskxnjclzjflaksjfdlaksjlaksjdasd");
            var stream = new MemoryStream(stringBytes);
         
            var hasher = new BuzHashRollingHasher(stream,32);

            uint hash = 0;
            while (!hasher.AtEnd)
            {
                hash = hasher.NextHash();
            }
            
            uint controlHash = new BuzHashFunction().Hash(stringBytes,stringBytes.Length-32,32);


            
            Assert.AreEqual(controlHash,hash);
        }
    }
}
