using System.Diagnostics;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace Miningcore.Blockchain.Bitcoin;

public static class BitcoinUtils
{
    public static IDestination AddressToDestination(string address, Network expectedNetwork)
    {
        try
        {
            var decoded = Encoders.Base58Check.DecodeData(address);
            var pubKeyVersion = expectedNetwork.GetVersionBytes(Base58Type.PUBKEY_ADDRESS, true);
            var scriptVersion = expectedNetwork.GetVersionBytes(Base58Type.SCRIPT_ADDRESS, true);

            if (pubKeyVersion != null && decoded.Take(pubKeyVersion.Length).SequenceEqual(pubKeyVersion))
            {
                decoded = decoded.Skip(pubKeyVersion.Length).ToArray();
                return new KeyId(decoded);
            }
            else if (scriptVersion != null && decoded.Take(scriptVersion.Length).SequenceEqual(scriptVersion))
            {
                decoded = decoded.Skip(scriptVersion.Length).ToArray();
                return new ScriptId(decoded);
            }

            throw new ArgumentException($"Unknown address type for network {expectedNetwork}");
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"Invalid Base58 address: {address}", ex);
        }
    }

    public static IDestination BechSegwitAddressToDestination(string address, Network expectedNetwork)
    {
        try
        {
            var encoder = expectedNetwork.GetBech32Encoder(Bech32Type.WITNESS_PUBKEY_ADDRESS, true);
            if (encoder == null)
                throw new ArgumentException($"Bech32 encoding not supported for network {expectedNetwork}");

            var decoded = encoder.Decode(address, out var witVersion);
            var result = new WitKeyId(decoded);

            Debug.Assert(result.GetAddress(expectedNetwork).ToString() == address, "Bech32 address verification failed");
            return result;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid Bech32 address: {address}", ex);
        }
    }

    public static IDestination BCashAddressToDestination(string address, Network expectedNetwork)
    {
        try
        {
            var chainName = expectedNetwork.Name.ToLower() switch
            {
                "mainnet" or "main" or "trrxitte" => ChainName.Mainnet,
                "testnet4" or "test4" => ChainName.Testnet,
                "regtest" or "reg" => ChainName.Regtest,
                _ => throw new ArgumentException("Unknown network chain name", nameof(expectedNetwork))
            };

            var bcashNetwork = NBitcoin.Altcoins.BCash.Instance.GetNetwork(chainName);

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
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid BCash address: {address}", ex);
        }
    }
}
