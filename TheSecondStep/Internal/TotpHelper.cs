// Copyright 2013 Nir Dobovizki
//
// Parts Copyright 2010 Google Inc. Author: Markus Gutschke  
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;

namespace TheSecondStep.Internal
{
    static class TotpHelper
    {
        private const int SHA1_DIGEST_LENGTH = 20;

        /// <summary>
        /// Current time, in number of half minutes since the begining of 1970
        /// </summary>
        public static int TimeStamp
        {
            get
            {
                TimeSpan span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
                double unixTime = span.TotalSeconds;
                return (int)Math.Floor(unixTime) / 30;
            }
        }


        public static string CreateNewSecret(int secretLengthInBits)
        {
            byte[] buf = new byte[secretLengthInBits / 8];

            System.Security.Cryptography.RNGCryptoServiceProvider rnd = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rnd.GetBytes(buf);

            return Base32Encoding.ToString(buf);
        }

        public static bool Authenticate(
            string secret,
            int timeWindowSize,
            int timestamp,
            int code) 
        {

            if (code < 0 || code >= 1000000) 
            {
                // All time based verification codes are no longer than six digits.
                return false;
            }

            bool result = false;
            // Compute verification codes and compare them with user input
            for (int i = -((timeWindowSize-1)/2); i <= timeWindowSize/2; ++i) 
            {
                int hash = GetVerificationCode(timestamp + i, secret);
                if (hash == (uint)code) 
                {
                    result = true;
                }
            }

            return result;
        }

        public static int GetVerificationCode(int value, string secretStr) 
        {   
            byte[] val = new byte[8];   
            for (int i = 8; (i--) != 0; value >>= 8) 
            {     
                val[i] = (byte)(value&0xFF);   
            }   

            byte[] secret = Base32Encoding.ToBytes(secretStr);
            if(secret == null || secret.Length==0)
            {
                return -1;
            }

            System.Security.Cryptography.HMACSHA1 hmac = new System.Security.Cryptography.HMACSHA1(secret,true);
            byte[] hash = hmac.ComputeHash(val);

            int offset = hash[SHA1_DIGEST_LENGTH - 1] & 0xF;   
            uint truncatedHash = 0;   
            for (int i = 0; i < 4; ++i) 
            {     
                truncatedHash <<= 8;     
                truncatedHash  |= hash[offset + i];   
            }   
            truncatedHash &= 0x7FFFFFFF;   
            truncatedHash %= 1000000;   
            return (int)truncatedHash; 
        }

        public static string GetSecretUrl(string secret, string label)
        {
            bool use_totp = true; // this class only supports TOTP, not HOTP
            string encodedLabel = System.Uri.EscapeDataString(label);
            string url = string.Format("otpauth://{0}otp/{1}?secret={2}", use_totp ? 't' : 'h', encodedLabel, secret);
            return url;
        }


    }
}
