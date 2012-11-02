using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

namespace Io.Rz.FlywheelComponents.Chunking
{
    [TestClass]
    public class ChunkerTests
    {
        [TestMethod]
        public void TestPenguins()
        {
            var listPenguinChunks = new List<byte[]>();
            var listPenguinChangedChunks = new List<byte[]>(); 
            
            using (FileStream fs = File.OpenRead(@"Resources\Penguins.jpg"))
            {
                Chunker c = new Chunker(fs, HasherType.BuzHash);

                
                foreach (byte[] chunk in c)
                {
                    listPenguinChunks.Add(chunk);
                }
               
            }
            using (FileStream fs = File.OpenRead(@"Resources\Penguins - Changed.jpg"))
            {
                Chunker c = new Chunker(fs, HasherType.BuzHash);


                foreach (byte[] chunk in c)
                {
                    listPenguinChangedChunks.Add(chunk);
                }

            }
            Assert.AreEqual(176, listPenguinChangedChunks.Count);
            Assert.AreEqual(176, listPenguinChunks.Count);

            for (var i = 0; i < 176; i++)
            {
                if (i == 0)
                    CollectionAssert.AreNotEqual(listPenguinChunks[i], listPenguinChangedChunks[i]);
                else
                    CollectionAssert.AreEqual(listPenguinChunks[i], listPenguinChangedChunks[i]);
            }
           
        }
    }
}
