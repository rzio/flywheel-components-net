using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Io.Rz.FlywheelComponents.Util
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



    /// <summary>
    /// 
    ///  Ported from http://webcat.sourceforge.net/javadocs/pt/tumba/parser/RabinHashFunction.html

    ///  We compute the checksum using Broder s implementation of
    ///  Rabin s fingerprinting algorithm. Fingerprints offer 
    ///  provably strong probabilistic guarantees that two 
    ///  different strings will not have the same fingerprint. 
    ///  Other checksum algorithms, such as MD5 and SHA, do not 
    ///  offer such provable guarantees, and are also more 
    ///  expensive to compute than Rabin fingerprint.
    ///  
    ///  A disadvantage is that these faster functions are 
    ///  efficiently invertible (that is, one can easily build an 
    ///  URL that hashes to a particular location), a fact that  
    ///  might be used by malicious users to nefarious purposes.
    ///  
    ///  Using the Rabin's fingerprinting function, the probability of
    ///  collision of two strings s1 and s2 can be bounded (in a adversarial
    ///  model for s1 and s2) by max(|s1|,|s2|)/2**(l-1), where |s1| is the 
    ///  length of the string s1 in bits.
    ///  
    ///  The advantage of choosing Rabin fingerprints (which are based on random
    ///  irreducible polynomials) rather than some arbitrary hash function is that
    ///  their probability of collision os well understood. Furthermore Rabin 
    ///  fingerprints can be computed very efficiently in software and we can
    ///  take advantage of their algebraic properties when we compute the
    ///  fingerprints of "sliding windows".
    ///  
    ///  M. O. Rabin
    ///  Fingerprinting by random polynomials.
    ///  Center for Research in Computing Technology
    ///  Harvard University Report TR-15-81
    ///  1981
    ///  
    ///  A. Z. Broder
    ///  Some applications of Rabin's fingerprinting method
    ///  In R.Capicelli, A. De Santis and U. Vaccaro editors
    ///  Sequences II:Methods in Communications, Security, and Computer Science
    ///  pages 143-152
    ///  Springer-Verlag
    ///  1993
    /// </summary>
    // TODO - need to figure out how to calculate rolling hash 
    public class RabinHashFunction
    {

        private readonly static uint P_DEGREE = 64;
        private readonly static uint READ_BUFFER_SIZE = 2048;
        private readonly static uint X_P_DEGREE = (uint)1 << ((int)P_DEGREE - 1);

        private readonly byte[] buffer;

        private ulong POLY = 0x0060034000F0D50A;

        private readonly ulong[] table32, table40, table48, table54;
        private readonly ulong[] table62, table70, table78, table84;

        /// <summary>
        ///  Constructor for the RabinHashFunction64 object
        /// </summary>
        public RabinHashFunction()
        {
            table32 = new ulong[256];
            table40 = new ulong[256];
            table48 = new ulong[256];
            table54 = new ulong[256];
            table62 = new ulong[256];
            table70 = new ulong[256];
            table78 = new ulong[256];
            table84 = new ulong[256];
            buffer = new byte[READ_BUFFER_SIZE];
            ulong[] mods = new ulong[P_DEGREE];
            mods[0] = POLY;
            for (int i = 0; i < 256; i++)
            {
                table32[i] = 0;
                table40[i] = 0;
                table48[i] = 0;
                table54[i] = 0;
                table62[i] = 0;
                table70[i] = 0;
                table78[i] = 0;
                table84[i] = 0;
            }
            for (int i = 1; i < P_DEGREE; i++)
            {
                mods[i] = mods[i - 1] << 1;
                if ((mods[i - 1] & X_P_DEGREE) != 0)
                {
                    mods[i] = mods[i] ^ POLY;
                }
            }
            for (int i = 0; i < 256; i++)
            {
                long c = i;
                for (int j = 0; j < 8 && c != 0; j++)
                {
                    if ((c & 1) != 0)
                    {
                        table32[i] = table32[i] ^ mods[j];
                        table40[i] = table40[i] ^ mods[j + 8];
                        table48[i] = table48[i] ^ mods[j + 16];
                        table54[i] = table54[i] ^ mods[j + 24];
                        table62[i] = table62[i] ^ mods[j + 32];
                        table70[i] = table70[i] ^ mods[j + 40];
                        table78[i] = table78[i] ^ mods[j + 48];
                        table84[i] = table84[i] ^ mods[j + 56];
                    }
                    c >>= 1;
                }
            }
        }

        /// <summary>
        /// Return the Rabin Hash value of an array of bytes.
        ///
        /// <param name="bytes"> the array of bytes</param>
        /// <returns>the Hash value></returns>
        /// </summary>
        public ulong Hash(byte[] bytes)
        {
            return Hash(bytes, 0, bytes.Length, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public ulong Hash(byte[] bytes, int offset, int length, ulong seed)
        {
            ulong w = seed;
            int start = length % 8;
            for (int s = offset; s < offset + start; s++)
            {
                w = (w << 8) ^ (bytes[s] & (uint)0xFF);
            }
            for (int s = offset + start; s < length + offset; s += 8)
            {
                w =
                    table32[(int)(w & 0xFF)]
                        ^ table40[(uint)((w >> 8) & 0xFF)]
                        ^ table48[(uint)((w >> 16) & 0xFF)]
                        ^ table54[(uint)((w >> 24) & 0xFF)]
                        ^ table62[(uint)((w >> 32) & 0xFF)]
                        ^ table70[(uint)((w >> 40) & 0xFF)]
                        ^ table78[(uint)((w >> 48) & 0xFF)]
                        ^ table84[(uint)((w >> 56) & 0xFF)]
                        ^ (ulong)(bytes[s] << 56)
                        ^ (ulong)(bytes[s + 1] << 48)
                        ^ (ulong)(bytes[s + 2] << 40)
                        ^ (ulong)(bytes[s + 3] << 32)
                        ^ (ulong)(bytes[s + 4] << 24)
                        ^ (ulong)(bytes[s + 5] << 16)
                        ^ (ulong)(bytes[s + 6] << 8)
                        ^ (ulong)(bytes[s + 7]);
            }
            return w;
        }

        /// <summary>
        ///  Return the Rabin Hash value of an array of chars.
        ///
        ///<param name="chars">the array of chars</param>
        /// <returns>the Hash value</returns>
         /// </summary>
        public ulong Hash(char[] chars)
        {
            ulong w = 0;
            int start = chars.Length % 4;
            for (int s = 0; s < start; s++)
            {
                w = (w << 16) ^ (chars[s] & (uint)0xFFFF);
            }
            for (int s = start; s < chars.Length; s += 4)
            {
                w =
                    table32[(uint)(w & 0xFF)]
                        ^ table40[(uint)((w >> 8) & 0xFF)]
                        ^ table48[(uint)((w >> 16) & 0xFF)]
                        ^ table54[(uint)((w >> 24) & 0xFF)]
                        ^ table62[(uint)((w >> 32) & 0xFF)]
                        ^ table70[(uint)((w >> 40) & 0xFF)]
                        ^ table78[(uint)((w >> 48) & 0xFF)]
                        ^ table84[(uint)((w >> 56) & 0xFF)]
                        ^ ((ulong)(chars[s] & 0xFFFF) << 48)
                        ^ ((ulong)(chars[s + 1] & 0xFFFF) << 32)
                        ^ ((ulong)(chars[s + 2] & 0xFFFF) << 16)
                        ^ ((ulong)(chars[s + 3] & 0xFFFF));
            }
            return w;
        }



        /**
         *  Returns the Rabin Hash value of an array of integers. This method is the
         *  most efficient of all the Hash methods, so it should be used when
         *  possible.
         *
         *@param  A  array of integers
         *@return    the Hash value
         */
        public ulong Hash(uint[] A)
        {
            ulong w = 0;
            int start = 0;
            if (A.Length % 2 == 1)
            {
                w = A[0] & 0xFFFFFFFF;
                start = 1;
            }
            for (int s = start; s < A.Length; s += 2)
            {
                w =
                    table32[(uint)(w & 0xFF)]
                        ^ table40[(uint)((w >> 8) & (uint)0xFF)]
                        ^ table48[(uint)((w >> 16) & 0xFF)]
                        ^ table54[(uint)((w >> 24) & 0xFF)]
                        ^ table62[(uint)((w >> 32) & 0xFF)]
                        ^ table70[(uint)((w >> 40) & 0xFF)]
                        ^ table78[(uint)((w >> 48) & 0xFF)]
                        ^ table84[(uint)((w >> 56) & 0xFF)]
                        ^ ((ulong)(A[s] & 0xFFFFFFFF) << 32)
                        ^ (ulong)(A[s + 1] & 0xFFFFFFFF);
            }
            return w;
        }

        /**
         *  Returns the Rabin Hash value of an array of longs. This method is the
         *  most efficient of all the Hash methods, so it should be used when
         *  possible.
         *
         *@param  A  array of integers
         *@return    the Hash value
         */
        public ulong Hash(ulong[] A)
        {
            ulong w = 0;
            for (int s = 0; s < A.Length; s++)
            {
                w =
                    table32[(uint)(w & 0xFF)]
                        ^ table40[(uint)((w >> 8) & 0xFF)]
                        ^ table48[(uint)((w >> 16) & 0xFF)]
                        ^ table54[(uint)((w >> 24) & 0xFF)]
                        ^ table62[(uint)((w >> 32) & 0xFF)]
                        ^ table70[(uint)((w >> 40) & 0xFF)]
                        ^ table78[(uint)((w >> 48) & 0xFF)]
                        ^ table84[(uint)((w >> 56) & 0xFF)]
                        ^ (A[s]);
            }
            return w;
        }


        /**
         *  Computes the Rabin Hash value of a String.
         *
         *@param  s  the string to be Hashed
         *@return    the Hash value
         */
        public ulong Hash(String s)
        {
            return Hash(s.ToCharArray());
        }

    }
}
