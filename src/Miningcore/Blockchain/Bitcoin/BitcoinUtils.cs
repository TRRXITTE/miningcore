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
    }

    // Get the appropriate Bitcoin Cash network instance
    var bcashNetwork = NBitcoin.Altcoins.BCash.Instance.GetNetwork(chainName);

    // Prepend prefix if address doesn't contain a colon
    if (!address.Contains(":"))
    {
        address = prefix + address;
    }

    var pubKeyAddress = bcashNetwork.Parse<NBitcoin.Altcoins.BCash.BTrashPubKeyAddress>(address);
    return pubKeyAddress.ScriptPubKey.GetDestinationAddress(bcashNetwork);
}