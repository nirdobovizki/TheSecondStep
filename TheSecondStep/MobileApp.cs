// Copyright 2013 Nir Dobovizki
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

namespace TheSecondStep
{
    /// <summary>
    /// Support for two factor authentication using a mobile app like Google Authenticator
    /// </summary>
    public static class MobileApp
    {
        [Serializable]
        public class MobileAppUserSettings
        {
            private string _secret;

            /// <summary>
            /// The secret string used to generate the one time code
            /// </summary>
            public string Secret { get { return _secret; } set { _secret = value; } }
        }

        [Serializable]
        public class MobileAppSystemSettings
        {
            private int _secretLengthInBits = 160;
            private int _timeWindowSize = 9;

            /// <summary>
            /// Secret length in bits
            /// 
            /// Default value 160, Only affects CreateNewSecret()
            /// </summary>
            public int SecretLengthInBits
            {
                get { return _secretLengthInBits; }
                set
                {
                    if (value % 8 != 0) throw new ArgumentException("SecretLengthInBits must be devisable by 8");
                    _secretLengthInBits = value;
                }
            }

            /// <summary>
            /// Number of codes to try
            /// 
            /// If this is 3 the system will allow one extra code before and one
            /// extra code after the current code for a 30 second clock mismatch,
            /// setting this to 5 will increase teh time window to 1 minute in 
            /// every direction, etc.
            /// 
            /// Only affets Authenticate()
            /// 
            /// Valid valued 1 - 9, default value 3
            /// </summary>
            public int TimeWindowSize
            {
                get { return _timeWindowSize; }
                set
                {
                    if (value < 1 || value > 9) throw new ArgumentOutOfRangeException("valid values for time window are 1-9");
                    _timeWindowSize = value;
                }
            }

        }

        public static MobileAppSystemSettings DefaultSystemSettings
        {
            get { return new MobileAppSystemSettings(); }
        }

        private static Internal.InvalidCodes _invalidCodes = new Internal.InvalidCodes();

        /// <summary>
        /// Create a new secret for a user
        /// </summary>
        /// <param name="systemSettings">System settings</param>
        /// <returns>New user settings</returns>
        public static MobileAppUserSettings CreateNewSecret(MobileAppSystemSettings systemSettings)
        {
            return new MobileAppUserSettings() { Secret = Internal.TotpHelper.CreateNewSecret(systemSettings.SecretLengthInBits) };
        }

        /// <summary>
        /// Get the Url to be encoded in a QR code and scanned into the app
        /// </summary>
        /// <param name="systemSettings">System settings</param>
        /// <param name="userSettings">User settings</param>
        /// <param name="label">Label to show in the app</param>
        /// <returns>The Url</returns>
        public static string GetSecretUrl(MobileAppSystemSettings systemSettings, MobileAppUserSettings userSettings, string label)
        {
            return Internal.TotpHelper.GetSecretUrl(userSettings.Secret, label);
        }

        /// <summary>
        /// Check if a code is valid
        /// </summary>
        /// <param name="systemSettings">System settings</param>
        /// <param name="userSettings">User settings</param>
        /// <param name="code">Code to check</param>
        /// <returns>True if the code is valid, false if not</returns>
        public static bool Authenticate(MobileAppSystemSettings systemSettings, MobileAppUserSettings userSettings, int code)
        {
            int timestamp = Internal.TotpHelper.TimeStamp;
            bool valid = Internal.TotpHelper.Authenticate(userSettings.Secret, systemSettings.TimeWindowSize, timestamp, code);
            return valid && _invalidCodes.CheckAndAdd(code, timestamp, systemSettings.TimeWindowSize);
        }
    }
}
