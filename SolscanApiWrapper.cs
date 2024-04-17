internal class SolscanApiWrapper
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl = "https://pro-api.solscan.io/v1.0";
    public SolscanApiWrapper(string apiKey)
    {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Add("accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("token", apiKey);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.80 Safari/537.36");
    }

    /// <summary>
    /// Common function to assemble request url components(Parameters, Path & Header) and make call.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private async Task<string> BuildUrlAsync(string path, params string[] @param)
    {
        if(param == null || param.Length == 0)
            return $"{baseUrl}{path}";

        string query = string.Join("&", param);
        return $"{baseUrl}{path}?{query}";
    }
    private async Task<string> MakeRequestAsync(string url)
    {
        var response = await httpClient.GetAsync(url);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SOLSCAN] ERROR: {ex.Message} [{url}]");
        }
        return await response.Content.ReadAsStringAsync();
    }
    /// <summary>
    /// Retrieves information about the last blocks from the Solscan API.
    /// Related-to : https://public-api.solscan.io/block/last?limit=10
    /// </summary>
    /// <param name="limit">The maximum number of blocks to retrieve (default is 10).</param>
    /// <returns>A string representing the JSON response containing information about the last blocks.</returns>
    public async Task<string> GetLastBlockAsync(int limit = 10)
    {
        var url = await BuildUrlAsync("/block/last", $"limit={limit}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about transactions in a specific block.
    /// Related-to: https://public-api.solscan.io/block/transactions?block={{blockID}}&offset=0&limit=10
    /// </summary>
    /// <param name="limit">The maximum number of transactions to retrieve (default is 10).</param>
    /// <param name="offset">The offset for pagination (default is 0).</param>
    /// <param name="block">The ID of the block to retrieve transactions from.</param>
    /// <returns>A string representing the JSON response containing information about transactions in the specified block.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the block ID is not specified.</exception>
    public async Task<string> GetBlockTransactionsAsync(int limit = 10, int offset = 0, string block = null)
    {
        if (block == null)
            throw new ArgumentNullException(nameof(block), "Block not specified");

        var url = await BuildUrlAsync("/block/transactions", $"limit={limit}", $"offset={offset}", block == null ? $"block=" : $"block={block}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about a specific block.
    /// Related-to: https://public-api.solscan.io/block/{{blockID}}
    /// </summary>
    /// <param name="block">The ID of the block to retrieve information about.</param>
    /// <returns>A string representing the JSON response containing information about the specified block.</returns>
    /// <exception cref="ArgumentException">Thrown when the block ID is not a positive integer.</exception>
    public async Task<string> GetBlockInfoAsync(int block)
    {
        if (block <= 0)
            throw new ArgumentException("Block ID must be a positive integer.", nameof(block));

        var url = await BuildUrlAsync($"/block/{block}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about the last transactions.
    /// Related-to: https://public-api.solscan.io/transaction/last?limit=10
    /// </summary>
    /// <param name="limit">The maximum number of transactions to retrieve (default is 10).</param>
    /// <returns>A string representing the JSON response containing information about the last transactions.</returns>
    public async Task<string> GetLastTransactionAsync(int limit = 10)
    {
        var url = await BuildUrlAsync("/transaction/last", $"limit={limit}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about a specific transaction identified by its signature.
    /// Related-to: https://public-api.solscan.io/transaction/{{transactionSignature}}
    /// </summary>
    /// <param name="signature">The signature of the transaction to retrieve information about.</param>
    /// <returns>A string representing the JSON response containing information about the specified transaction.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the transaction signature is not specified.</exception>
    public async Task<string> GetTransactionSignatureInfoAsync(string signature)
    {
        if (signature == null)
            throw new ArgumentNullException(nameof(signature), "Signature not specified");

        var url = await BuildUrlAsync($"/transaction/{signature}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about tokens associated with a specific account.
    /// Related-to: https://public-api.solscan.io/account/tokens?account={{accountID}}
    /// </summary>
    /// <param name="account">The ID of the account to retrieve token information for.</param>
    /// <returns>A string representing the JSON response containing information about tokens associated with the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    public async Task<string> GetAccountTokensAsync(string account)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");

        var url = await BuildUrlAsync("/account/tokens",  $"account={account}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about transactions associated with a specific account.
    /// Related-to: https://public-api.solscan.io/account/transactions?account={{accountID}}&beforeHash={{beforeHash}}&limit=10
    /// </summary>
    /// <param name="account">The ID of the account to retrieve transaction information for.</param>
    /// <param name="beforeHash">Optional. Retrieve transactions before a specific hash.</param>
    /// <param name="limit">The maximum number of transactions to retrieve (default is 10).</param>
    /// <returns>A string representing the JSON response containing information about transactions associated with the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the limit is not a positive integer.</exception>
    public async Task<string> GetAccountTransactionsAsync(string account, string beforeHash = null, int limit = 10)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");
        if (limit <= 0)
            throw new ArgumentException("Limit must be a positive integer.", nameof(limit));

        var parameters = new List<string>
        {
            $"account={Uri.EscapeDataString(account)}",
            $"limit={limit}"
        };

        if (!string.IsNullOrEmpty(beforeHash))
            parameters.Add($"beforeHash={Uri.EscapeDataString(beforeHash)}");


        var url = await BuildUrlAsync("/account/transactions", parameters.ToArray());
        return await MakeRequestAsync(url);
    }


    /// <summary>
    /// Retrieves information about stake accounts associated with a specific account.
    /// Related-to: https://public-api.solscan.io/account/stakeAccounts?account={{accountID}}
    /// </summary>
    /// <param name="account">The ID of the account to retrieve stake account information for.</param>
    /// <returns>A string representing the JSON response containing information about stake accounts associated with the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    public async Task<string> GetAccountStakeAccountsAsync(string account)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");

        var url = await BuildUrlAsync($"/account/stakeAccounts?account={account}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about SPL transfers associated with a specific account.
    /// Related-to: https://public-api.solscan.io/account/splTransfers?account={{accountID}}&offset=0&limit=10
    /// </summary>
    /// <param name="account">The ID of the account to retrieve SPL transfer information for.</param>
    /// <param name="limit">The maximum number of SPL transfers to retrieve (default is 10).</param>
    /// <param name="offset">The offset for pagination (default is 0).</param>
    /// <param name="fromTime">Optional. The start time of the time range for which to retrieve SPL transfers.</param>
    /// <param name="toTime">Optional. The end time of the time range for which to retrieve SPL transfers.</param>
    /// <returns>A string representing the JSON response containing information about SPL transfers associated with the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the limit is not a positive integer.</exception>
    public async Task<string> GetAccountSplTransfersAsync(string account, int limit = 10, int offset = 0, int? fromTime = null, int? toTime = null)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");
        if (limit <= 0)
            throw new ArgumentException("Limit must be a positive integer.", nameof(limit));

        var parameters = new List<string>
        {
            $"account={Uri.EscapeDataString(account)}",
            $"offset={offset}",
            $"limit={limit}"
        };

        if (fromTime.HasValue)
            parameters.Add($"fromTime={fromTime.Value}");

        if (toTime.HasValue)
            parameters.Add($"toTime={toTime.Value}");

        var url = await BuildUrlAsync("/account/splTransfers", parameters.ToArray());
        return await MakeRequestAsync(url);
    }


    /// <summary>
    /// Retrieves information about SOL transfers associated with a specific account.
    /// Related-to: https://public-api.solscan.io/account/solTransfers?account={{accountID}}&offset=0&limit=10
    /// </summary>
    /// <param name="account">The ID of the account to retrieve SOL transfer information for.</param>
    /// <param name="limit">The maximum number of SOL transfers to retrieve (default is 10).</param>
    /// <param name="offset">The offset for pagination (default is 0).</param>
    /// <param name="fromTime">Optional. The start time of the time range for which to retrieve SOL transfers.</param>
    /// <param name="toTime">Optional. The end time of the time range for which to retrieve SOL transfers.</param>
    /// <returns>A string representing the JSON response containing information about SOL transfers associated with the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the limit is not a positive integer.</exception>
    public async Task<string> GetAccountSolTransfersAsync(string account, int limit = 10, int offset = 0, int? fromTime = null, int? toTime = null)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");
        if (limit <= 0)
            throw new ArgumentException("Limit must be a positive integer.", nameof(limit));

        var parameters = new List<string>
        {
            $"account={Uri.EscapeDataString(account)}",
            $"offset={offset}",
            $"limit={limit}"
        };

        if (fromTime.HasValue)
            parameters.Add($"fromTime={fromTime.Value}");

        if (toTime.HasValue)
            parameters.Add($"toTime={toTime.Value}");

        var url = await BuildUrlAsync("/account/solTransfers", parameters.ToArray());
        return await MakeRequestAsync(url);
    }


    /// <summary>
    /// Retrieves exported transaction information associated with a specific account.
    /// Related-to: https://public-api.solscan.io/account/exportTransactions?account={{accountID}}&type={{tokenchange|solTransfers|all}}&fromTime={{timestamp}}&toTime={{timestamp}}
    /// </summary>
    /// <param name="account">The ID of the account to retrieve exported transaction information for.</param>
    /// <param name="type">Optional. The type of transactions to retrieve (default is "all").</param>
    /// <param name="fromTime">Optional. The start time of the time range for which to retrieve transactions.</param>
    /// <param name="toTime">Optional. The end time of the time range for which to retrieve transactions.</param>
    /// <returns>A string representing the JSON response containing exported transaction information associated with the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    public async Task<string> GetAccountExportTransactionsAsync(string account, string type = "all", int? fromTime = null, int? toTime = null)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");

        var parameters = new List<string>
        {
            $"account={Uri.EscapeDataString(account)}",
            $"type={Uri.EscapeDataString(type)}"
        };

        if (fromTime.HasValue)
            parameters.Add($"fromTime={fromTime.Value}");

        if (toTime.HasValue)
            parameters.Add($"toTime={toTime.Value}");

        var url = await BuildUrlAsync("/account/exportTransactions", parameters.ToArray());
        return await MakeRequestAsync(url);
    }


    /// <summary>
    /// Retrieves information about a specific account.
    /// Related-to: https://public-api.solscan.io/account/{{accountID}}
    /// </summary>
    /// <param name="account">The ID of the account to retrieve information for.</param>
    /// <returns>A string representing the JSON response containing information about the specified account.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the account ID is null or empty.</exception>
    public async Task<string> GetAccountInfoAsync(string account)
    {
        if (string.IsNullOrEmpty(account))
            throw new ArgumentNullException(nameof(account), "Account ID must not be null or empty.");

        var url = await BuildUrlAsync($"/account/{account}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about holders of a specific token.
    /// Related-to: https://public-api.solscan.io/token/holders?tokenAddress={{tokenaddr}}&offset=0&limit=10
    /// </summary>
    /// <param name="tokenAddress">The address of the token to retrieve holder information for.</param>
    /// <param name="limit">The maximum number of holders to retrieve (default is 10).</param>
    /// <param name="offset">The offset for pagination (default is 0).</param>
    /// <returns>A string representing the JSON response containing information about holders of the specified token.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the token address is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the limit is not a positive integer.</exception>
    public async Task<string> GetTokenHoldersAsync(string tokenAddress, int limit = 10, int offset = 0)
    {
        if (string.IsNullOrEmpty(tokenAddress))
            throw new ArgumentNullException(nameof(tokenAddress), "Token address must not be null or empty.");
        if (limit <= 0)
            throw new ArgumentException("Limit must be a positive integer.", nameof(limit));

       var url = await BuildUrlAsync("/token/holders", 
           $"tokenAddress={tokenAddress}", 
           $"limit={limit}", 
           $"offset={offset}");

        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves metadata information about a specific token.
    /// Related-to: https://public-api.solscan.io/token/meta?tokenAddress={{tokenaddr}}
    /// </summary>
    /// <param name="tokenAddress">The address of the token to retrieve metadata for.</param>
    /// <returns>A string representing the JSON response containing metadata information about the specified token.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the token address is null or empty.</exception>
    public async Task<string> GetTokenMetaAsync(string tokenAddress)
    {
        if (string.IsNullOrEmpty(tokenAddress))
            throw new ArgumentNullException(nameof(tokenAddress), "Token address must not be null or empty.");

        var url = await BuildUrlAsync($"/token/meta/?tokenAddress={tokenAddress}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves a list of tokens with optional sorting and pagination.
    /// Related-to: https://public-api.solscan.io/token/list?sortBy={{market_cap | volume | holder | price | price_change_24h | price_change_7d | price_change_14d | price_change_30d | price_change_60d | price_change_200d | price_change_1y}}&direction=desc&limit=10&offset=0
    /// </summary>
    /// <param name="sortBy">Optional. The field to sort tokens by (default is "market_cap").</param>
    /// <param name="direction">Optional. The direction of sorting (default is "desc").</param>
    /// <param name="limit">Optional. The maximum number of tokens to retrieve (default is 10).</param>
    /// <param name="offset">Optional. The offset for pagination (default is 0).</param>
    /// <returns>A string representing the JSON response containing a list of tokens.</returns>
    public async Task<string> GetTokenListAsync(string sortBy = "market_cap", string direction = "desc", int limit = 10, int offset = 0)
    {
        var url = await BuildUrlAsync("/token/list", 
            $"sortBy={sortBy}", 
            $"direction={direction}", 
            $"limit={limit}", 
            $"offset={offset}");

        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves market information about a specific token.
    /// Related-to: https://public-api.solscan.io/market/token/{{tokenaddr}}
    /// </summary>
    /// <param name="tokenAddress">The address of the token to retrieve market information for.</param>
    /// <returns>A string representing the JSON response containing market information about the specified token.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the token address is null or empty.</exception>
    public async Task<string> GetMarketTokenInfoAsync(string tokenAddress)
    {
        if (string.IsNullOrEmpty(tokenAddress))
            throw new ArgumentNullException(nameof(tokenAddress), "Token address must not be null or empty.");

        var url = await BuildUrlAsync($"/market/token/{tokenAddress}");
        return await MakeRequestAsync(url);
    }

    /// <summary>
    /// Retrieves information about the blockchain.
    /// Related-to: https://public-api.solscan.io/chaininfo/
    /// </summary>
    /// <returns>A string representing the JSON response containing information about the blockchain.</returns>
    public async Task<string> GetChainInfoAsync()
    {
        var url = await BuildUrlAsync("/chaininfo");
        return await MakeRequestAsync(url);
    }

}
