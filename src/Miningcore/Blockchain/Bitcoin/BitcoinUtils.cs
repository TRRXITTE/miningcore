using System.Diagnostics;
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
