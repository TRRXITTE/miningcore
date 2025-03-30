using System.Diagnostics;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Miningcore.Blockchain.Bitcoin;

public static class BitcoinUtils
{
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
        // Map the expectedNetwork's ChainName to the BCash library's ChainName
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

        var pubKeyAddress = bcashNetwork.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
        return pubKeyAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork);
    }

    /// <summary>
    /// Adds support for DigiByte addresses (Base58Check P2PKH starting with 'D', P2SH starting with '3', and Bech32 starting with 'dgb1').
    /// </summary>
    public static IDestination DigiByteAddressToDestination(string address, Network expectedNetwork)
    {
        // Check if it's a Bech32 SegWit address (starts with 'dgb1')
        if (address.StartsWith("dgb1"))
        {
            var encoder = expectedNetwork.GetBech32Encoder(Bech32Type.WITNESS_PUBKEY_ADDRESS, true);
            var bech32Decoded = encoder.Decode(address, out var witVersion);
            var bech32Result = new WitKeyId(bech32Decoded);
            Debug.Assert(bech32Result.GetAddress(expectedNetwork).ToString() == address);
            return bech32Result;
        }

        // Handle legacy Base58Check addresses (P2PKH starts with 'D', P2SH starts with '3')
        var base58Decoded = Encoders.Base58Check.DecodeData(address);
        byte[] versionBytes;

        // DigiByte P2PKH addresses start with 'D' (version byte 0x1E), P2SH with '3' (version byte 0x05)
        if (address.StartsWith("D"))
            versionBytes = expectedNetwork.GetVersionBytes(Base58Type.PUBKEY_ADDRESS, true); // 0x1E for DigiByte Mainnet
        else if (address.StartsWith("3"))
            versionBytes = expectedNetwork.GetVersionBytes(Base58Type.SCRIPT_ADDRESS, true); // 0x05
        else
            throw new FormatException("Invalid DigiByte address prefix");

        // Validate version byte and extract payload
        if (base58Decoded[0] != versionBytes[0])
            throw new FormatException("Address version byte does not match expected network");

        base58Decoded = base58Decoded.Skip(versionBytes.Length).ToArray();
        var base58Result = new KeyId(base58Decoded);
        return base58Result;
    }
}