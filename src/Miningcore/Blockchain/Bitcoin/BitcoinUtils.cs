<<<<<<< HEAD
using System.Diagnostics;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Miningcore.Blockchain.Bitcoin;

public static class BitcoinUtils
{
    /// <summary>
    /// Bitcoin addresses are implemented using the Base58Check encoding of the hash of either:
    /// Pay-to-script-hash (p2sh): payload is: RIPEMD160(SHA256(redeemScript)) where redeemScript is a
    /// script the wallet knows how to spend; version byte = 0x05 (these addresses begin with the digit '3')
    /// Pay-to-pubkey-hash (p2pkh): payload is RIPEMD160(SHA256(ECDSA_publicKey)) where
    /// ECDSA_publicKey is a public key the wallet knows the private key for; version byte = 0x00
    /// (these addresses begin with the digit '1')
    /// The resulting hash in both of these cases is always exactly 20 bytes.
    /// </summary>
    public static IDestination AddressToDestination(string address, Network expectedNetwork)
    {
        var decoded = Encoders.Base58Check.DecodeData(address);
        var networkVersionBytes = expectedNetwork.GetVersionBytes(Base58Type.PUBKEY_ADDRESS, true)
            ?? throw new ArgumentException("Invalid network version bytes for PUBKEY_ADDRESS", nameof(expectedNetwork));
        decoded = decoded.Skip(networkVersionBytes.Length).ToArray();
        var result = new KeyId(decoded);

        return result;
=======
public static IDestination BCashAddressToDestination(string address, Network expectedNetwork)
{
    // Map the expectedNetwork's ChainName to BCash ChainName and prefix
    string chainNameStr = expectedNetwork.ChainName.ToString().ToLower();
    ChainName chainName;
    string prefix;

    switch (chainNameStr)
    {
        case "mainnet":
        case "main":
            chainName = ChainName.Mainnet;
            prefix = "bitcoincash:";
            break;
        case "testnet4":
        case "test4":
            chainName = ChainName.Testnet;
            prefix = "bchtest:";
            break;
        case "regtest":
        case "reg":
            chainName = ChainName.Regtest;
            prefix = "bchreg:";
            break;
        default:
            throw new ArgumentException("Unknown network chain name", nameof(expectedNetwork));
>>>>>>> parent of 83766f40 (Update BitcoinUtils.cs)
    }

    public static IDestination BechSegwitAddressToDestination(string address, Network expectedNetwork)
    {
<<<<<<< HEAD
        var encoder = expectedNetwork.GetBech32Encoder(Bech32Type.WITNESS_PUBKEY_ADDRESS, true)
            ?? throw new ArgumentException("Bech32 encoder not available for the specified network", nameof(expectedNetwork));
        var decoded = encoder.Decode(address, out var witVersion);
        var result = new WitKeyId(decoded);

        Debug.Assert(result.GetAddress(expectedNetwork).ToString() == address, "Generated address does not match input");
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
        var bcashNetwork = NBitcoin.Altcoins.BCash.Instance.GetNetwork(chainName)
            ?? throw new ArgumentException("BCash network not found for the specified chain", nameof(chainName));

        // If the address doesn't contain a colon, assume it's missing the CashAddr prefix.
        if (!address.Contains(':'))
        {
            address = chainName switch
            {
                ChainName.Mainnet => "bitcoincash:" + address,
                ChainName.Testnet => "bchtest:" + address,
                ChainName.Regtest => "bchreg:" + address,
                _ => throw new ArgumentException("Unexpected chain name after mapping", nameof(chainName))
            };
        }

        var pubKeyAddress = bcashNetwork.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
        return pubKeyAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork);
    }

    /// <summary>
    /// Adds support for DigiByte Bech32 addresses (starting with 'dgb1').
    /// This method handles SegWit P2WPKH addresses for DigiByte.
    /// </summary>
    public static IDestination DigiByteAddressToDestination(string address, Network expectedNetwork)
    {
        // Validate that the address is a DigiByte Bech32 address starting with 'dgb1'
        if (string.IsNullOrEmpty(address) || !address.StartsWith("dgb1"))
        {
            throw new FormatException("DigiByte address must start with 'dgb1' for Bech32 support");
        }

        // Get the Bech32 encoder for the network
        var encoder = expectedNetwork.GetBech32Encoder(Bech32Type.WITNESS_PUBKEY_ADDRESS, true)
            ?? throw new ArgumentException("Bech32 encoder not available for the specified DigiByte network", nameof(expectedNetwork));

        // Decode the Bech32 address
        var decoded = encoder.Decode(address, out byte witVersion);

        // Ensure the witness version is valid (typically 0 for P2WPKH)
        if (witVersion != 0)
        {
            throw new FormatException($"Unsupported witness version {witVersion} for DigiByte Bech32 address");
        }

        // Create the WitKeyId from the decoded data
        var result = new WitKeyId(decoded);

        // Verify the address matches (optional, for debugging)
        Debug.Assert(result.GetAddress(expectedNetwork).ToString() == address, "Generated DigiByte address does not match input");

        return result;
    }
=======
        address = prefix + address;
    }

    var pubKeyAddress = bcashNetwork.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
    return pubKeyAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork);
>>>>>>> parent of 83766f40 (Update BitcoinUtils.cs)
}using System.Diagnostics;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Miningcore.Blockchain.Bitcoin;

public static class BitcoinUtils
{
    /// <summary>
    /// Bitcoin addresses are implemented using the Base58Check encoding of the hash of either:
    /// Pay-to-script-hash(p2sh): payload is: RIPEMD160(SHA256(redeemScript)) where redeemScript is a
    /// script the wallet knows how to spend; version byte = 0x05 (these addresses begin with the digit '3')
    /// Pay-to-pubkey-hash(p2pkh): payload is RIPEMD160(SHA256(ECDSA_publicKey)) where
    /// ECDSA_publicKey is a public key the wallet knows the private key for; version byte = 0x00
    /// (these addresses begin with the digit '1')
    /// The resulting hash in both of these cases is always exactly 20 bytes.
    /// </summary>
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
            "mainnet" or "main"     => ChainName.Mainnet,
            "testnet4" or "test4"     => ChainName.Testnet,
            "regtest" or "reg"        => ChainName.Regtest,
            _ => throw new ArgumentException("Unknown network chain name", nameof(expectedNetwork))
        };

        // Get the appropriate Bitcoin Cash network instance
        var bcashNetwork = NBitcoin.Altcoins.BCash.Instance.GetNetwork(chainName);

        // If the address doesn't contain a colon, assume it's missing the CashAddr prefix.
        if (!address.Contains(":"))
        {
            if (chainName == ChainName.Mainnet)
                address = "bitcoincash:" + address;
            else if (chainName == ChainName.Testnet)
                address = "bchtest:" + address;
            else if (chainName == ChainName.Regtest)
                address = "bchreg:" + address;
        }

        var pubKeyAddress = bcashNetwork.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
        return pubKeyAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork);
    }
}