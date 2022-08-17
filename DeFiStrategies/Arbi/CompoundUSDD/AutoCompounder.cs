using System.Numerics;
using DeFi_Strategies.Helpers;
using Nethereum.HdWallet;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using Nethereum;

namespace DeFi_Strategies.Tron.CompoundRDNT
{
    public class AutoCompounder
    {
        public const string GaugeAddress = "0xc2054A8C33bfce28De8aF4aF548C48915c455c13";

        public readonly string AlchemyApiEndpoint = "https://arb-mainnet.g.alchemy.com/v2/";

        private readonly string accountMnemonic;
        private readonly double claimThresholdRDNT;
       
        private readonly string accountAddress;
        private readonly Web3 web3;
        private readonly Account mainAccount;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AutoCompounder(CompoundingConfig config)
        {
            this.accountMnemonic = config.AccountMnemonic;
            this.claimThresholdRDNT = config.ClaimThresholdRDNT;
            this.AlchemyApiEndpoint =  "https://arb-mainnet.g.alchemy.com/v2/" + config.AlchemyAPIKey;
            // Setup account
            Wallet wallet1 = new Wallet(accountMnemonic, null);
            this.mainAccount = wallet1.GetAccount(0, 42161);
            
            this.accountAddress = mainAccount.Address;
            this.web3 = new Web3(mainAccount, AlchemyApiEndpoint);
           
        }

        public async Task AutocompoundAsync()
        {
      
            this.logger.Info("Autocompounding started. Your address: " + accountAddress);
            SuperGaugeContractClient gauge = new SuperGaugeContractClient(this.web3, GaugeAddress, accountAddress, this.mainAccount.PrivateKey);

            while (true)
            {
                try
                {
                    double claimableRDNT = await gauge.GetClaimableRewardsAsync(accountAddress);

                    this.logger.Info("Claimable RDNT: {0}", claimableRDNT);

                    if (claimableRDNT < claimThresholdRDNT)
                        logger.Info(string.Format("Claimable RDNT ({0}) is less than threshold ({1}). Not claiming.", claimableRDNT, claimThresholdRDNT));
                    else
                    {
                        this.logger.Info(string.Format("Claiming {0} RDNT rewards and waiting 30 sec for state update...", claimableRDNT));
                        string[] addresses = { "0x0C4681e6C0235179ec3D4F4fc4DF3d14FDD96017", "0x5293c6CA56b8941040b8D18f557dFA82cF520216", "0x805ba50001779CeD4f59CfF63aea527D12B94829", "0xEf47CCC71EC8941B67DC679D1a5f78fACfD0ec3C", "0x15b53d277Af860f51c3E6843F8075007026BBb3a", "0x4cD44E6fCfA68bf797c65889c74B26b8C2e5d4d3" };
                        string claimTxId = await gauge.ClaimRewardsAsync(addresses);

                        if (claimTxId == null)
                            this.logger.Warn("Claim tx id is null! Error");
                        else
                            this.logger.Debug("Claim txid: " + claimTxId);

                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }

                    AccountBalance balanceInfo = await this.GetBalancesInfoAsync();
                    balanceInfo.Log(this.logger);

                    if ((double)balanceInfo.UsdBalance < claimThresholdRDNT)
                    {
                        this.logger.Info(string.Format("RDNT balance is less than threshold of {0}. Waiting 60 minutes for next iteration...", claimThresholdRDNT));
                        await Task.Delay(TimeSpan.FromMinutes(60));
                        continue;
                    }


                    balanceInfo = await this.GetBalancesInfoAsync();
                    balanceInfo.Log(this.logger);

                    logger.Info("Staking {0} tokens.", balanceInfo.Balance_RDNT);

                    string depositLPTxId = await gauge.StakeTokensAsync(balanceInfo.Balance_RDNT, accountAddress);

                    if (depositLPTxId == null)
                        this.logger.Warn("Stake tx id is null! Error");
                    else
                        this.logger.Debug("Stake LP txid: " + depositLPTxId);
                }
                catch (Exception e)
                {
                    this.logger.Error(e.ToString());
                }

                logger.Info("Waiting 60 minutes...");
                await Task.Delay(TimeSpan.FromMinutes(60));
            }
        }

        private async Task<AccountBalance> GetBalancesInfoAsync()
        {
            Uri accInfoEndpoint = new Uri("https://api.debank.com/token/balance_list?user_addr=" + accountAddress + "&is_all=false&chain=arb");
            RestClient client = new RestClient(accInfoEndpoint);

            RestRequest request = new RestRequest(accInfoEndpoint);
            request.AddHeader("Accept", "application/json");

            RestResponse response = await client.ExecuteAsync<AccountInfoRoot>(request);

            AccountInfoRoot rootObj = JsonConvert.DeserializeObject<AccountInfoRoot>(response.Content);
          
            decimal ethBalance = 0;
            decimal RDNTBalance = 0;
            decimal usdBalance = 0;
            decimal price = 0;

            foreach (var data in rootObj.data)
            {
                if (data.name == "ETH")
                {

                    ethBalance = data.balance == null ? 0 : data.balance / 1000000000000000000;
                }

                if (data.name == "Radiant")
                {
                    RDNTBalance = data.balance == null ? 0 : data.balance / 1000000000000000000;
                    price = data.price == null ? 0 : data.price;
                    usdBalance = RDNTBalance * price;
                }
            }


            return new AccountBalance()
            {
                Balance_ETH = ethBalance,
                Balance_RDNT = RDNTBalance,
                UsdBalance = usdBalance
            };
        }
    }

    internal class AccountBalance
    {
        public decimal Balance_RDNT, Balance_ETH, UsdBalance;

        public void Log(Logger logger)
        {
            logger.Info("Balance_RDNT: {0}  Balance_ETH: {1} USD_BALANCE_RDNT: {2}", Balance_RDNT, Balance_ETH, UsdBalance);
        }
    }
}
