using System.Diagnostics;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Miningcore.Blockchain.Bitcoin;

public static class BitcoinUtils
{
    // Existing methods (unchanged)
    public static IDestination AddressToDestination(string address, Network expectedNetwork)
    {
        var decoded = Encoders.Base58Check.DecodeData(address);
        var networkVersionBytes = expectedNetwork.GetVersionBytes(Base58Type.PUBKEY_ADDRESS, true);
        decoded = decoded.Skip(networkVersionBytes.Length).ToArray();
        var result = new KeyId(decoded);
        return result;
    }

    public static IDestination BechSegwitAddressToDestination(string address, Network expectedNetwork)
    {
        var encoder = expectedNetwork.GetBech32Encoder(Bech32Type.WITNESS_PUBKEY_ADDRESS, true);
        var decoded = encoder.Decode(address, out var witVersion);
        var result = new WitKeyId(decoded);
        Debug.Assert(result.GetAddress(expectedNetwork).ToString() == address);
        return result;
    }

    public static IDestination BCashAddressToDestination(string address, Network expectedNetwork)
    {
        // Map the expectedNetwork's ChainName to BCash library's ChainName
        ChainName chainName = expectedNetwork.ChainName.ToString().ToLower() switch
        {
            "mainnet" or "main" => ChainName.Mainnet,
            "testnet4" or "test4" => ChainName.Testnet,
            "regtest" or "reg" => ChainName.Regtest,
            _ => throw new ArgumentException("Unknown network chain name", nameof(expectedNetwork))
        };

        // Get the appropriate Bitcoin Cash network instance
        var bcashNetwork = NBitcoin.Altcoins.BCash.Instance.GetNetwork(chainName);

        // If the address doesn't contain a colon, prepend the appropriate prefix
        if (!address.Contains(":"))
        {
            string prefix = chainName switch
            {
                ChainName.Mainnet => "bitcoincash:",
                ChainName.Testnet => "bchtest:",
                ChainName.Regtest => "bchreg:",
                _ => throw new ArgumentException("Unexpected chain name after mapping", nameof(chainName))
            };
            address = prefix + address;
        }

        // Parse the address as a Bitcoin Cash address
        var pubKeyAddress = bcashNetwork.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
        return pubKeyAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork);
    }
}