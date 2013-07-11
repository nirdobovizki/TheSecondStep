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
using System.Collections.Generic;
using System.Linq;

namespace TheSecondStep.Internal
{
    class InvalidCodes
    {
        private object _lock = new object();
        private Dictionary<int, int> _invalidCodes = new Dictionary<int,int>();

        public bool CheckAndAdd(int code, int timestamp, int windowSize)
        {
            lock (_lock)
            {
                foreach (var toRemove in _invalidCodes.Where(x => x.Value < timestamp).Select(x => x.Key).ToList())
                {
                    _invalidCodes.Remove(toRemove);
                }
                int dummy;
                if (_invalidCodes.TryGetValue(code, out dummy))
                {
                    return false;
                }
                _invalidCodes.Add(code, timestamp + windowSize);
                return true;
            }
        }
    }
}
