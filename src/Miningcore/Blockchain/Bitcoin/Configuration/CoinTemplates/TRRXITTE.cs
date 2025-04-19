using System.Collections.Generic;

namespace Miningcore.Blockchain.Bitcoin.Configuration.CoinTemplates
{
    public class TRRXITTE
    {
        public string Name { get; } = "trrxitte-bitcoin";
        public string Symbol { get; } = "TRRXITTE";
        public string Algorithm { get; } = "SHA256";
        public bool SupportsSegwit { get; } = true;
        public bool SupportsTaproot { get; } = true;
        public Dictionary<string, string> AddressPrefixes { get; } = new Dictionary<string, string>
        {
            { "p2pkh", "tb1q" },
            { "p2sh", "w" },
            { "p2tr", "tb1p" }
        };
        public uint Magic { get; } = 0xa1a4b5c3; // TRRXITTE magic number
        public decimal BlockReward { get; } = 100; // Matches block 2060 reward
    }
}
