using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Core.Common.Contracts
{
    public class SharedRawData
    {
        public string Id { get; set; }

        public string Version { get; set; }
        
        public string Hash { get; set; }

        public int LastTimestamp { get; set; }

        public IDictionary<string, byte[]> Data { get; set; }

        public static SharedRawData MergeSharedRawData(SharedRawData old, SharedRawData updated)
        {
            var sharedRawData = new SharedRawData
            {
                Hash = updated?.Hash ?? old?.Hash,
                Id = updated?.Id ?? old?.Id,
                Version = updated?.Version ?? old?.Version,
                LastTimestamp = Math.Max(old?.LastTimestamp ?? 0, updated?.LastTimestamp ?? 0),
                Data = old?.Data.ToDictionary(pair => pair.Key, pair => pair.Value) ?? (updated != null ? updated.Data.ToDictionary(pair => pair.Key, pair => pair.Value) : null)
            };
            if (old == null || updated == null)
                return sharedRawData;
            foreach (var keyValuePair in updated.Data)
                sharedRawData.Data[keyValuePair.Key] = keyValuePair.Value;
            return sharedRawData;
        }

    }
}