namespace Miningcore.Blockchain.Bitcoin.DaemonResponses
{
    public class ValidateAddressResult
    {
        public bool IsValid { get; set; }
        public bool IsBech32 { get; set; }
        public bool IsScript { get; set; }
        public string Address { get; set; }
    }
}
