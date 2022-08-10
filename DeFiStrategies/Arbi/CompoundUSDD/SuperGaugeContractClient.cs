using System.Globalization;
using DeFi_Strategies.Helpers;
using NLog;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.ABI;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using BigInteger = System.Numerics.BigInteger;

namespace DeFi_Strategies.Tron.CompoundRDNT
{
    public class SuperGaugeContractClient
    {
        private readonly Web3 web3;
        private readonly string contractAddress;
        private readonly string accountAddress;
        private readonly string privateKey;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SuperGaugeContractClient(Web3 web3, string contractAddress, string accountAddress, string privateKey)
        {
            this.web3 = web3;
            this.contractAddress = contractAddress;
            this.accountAddress = accountAddress;
            this.privateKey = privateKey;
            web3.TransactionManager.UseLegacyAsDefault = true;
        }

        [Function("getReward")]
        public class GetRewardFunction : FunctionMessage
        {
            [Parameter("address[]", "_rewardTokens", 1)]
            public string[] addresses { get; set; }
        }

        public async Task<string> ClaimRewardsAsync(string[] Addresses)
        {

            //FunctionABI functionABI = ABITypedRegistry.GetFunctionABI<GetRewardFunction>();
           

            Nethereum.ABI.Model.FunctionABI functionABI = Nethereum.Contracts.ABITypedRegistry.GetFunctionABI<GetRewardFunction>();

            try
            {
                GetRewardFunction claim = new GetRewardFunction()
                {
                    addresses = Addresses
                };

                
                //var claimHandler = web3.Eth.GetContractQueryHandler<GetRewardFunction>();
                var claimHandler = web3.Eth.GetContractTransactionHandler<GetRewardFunction>();
                var estimate = await claimHandler.EstimateGasAsync(contractAddress, claim);
                claim.Gas =  estimate.Value;
                //claim.SetTransactionType1559();
               
                //this.logger.Info("Autocompounding started. Your gas: " + estimate.Value);
                var txid = await claimHandler.SendRequestAndWaitForReceiptAsync(contractAddress, claim);
              
                return txid.TransactionHash;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, ex.Message);
                return null;
            }
        }

        [Function("claimableRewards", "uint256")]
        public class ClaimableRewardsFunction : FunctionMessage
        {
            [Parameter("address", "account", 1)]
            public string addr_address { get; set; }
        }

        public async Task<double> GetClaimableRewardsAsync(string address)
        {
            /*byte[] contractAddressBytes = Base58Encoder.DecodeFromBase58Check(contractAddress);
            byte[] ownerAddressBytes = Base58Encoder.DecodeFromBase58Check(accountAddress);
            FunctionABI functionABI = ABITypedRegistry.GetFunctionABI<ClaimableRewardsFunction>();

            byte[] addressBytes = new byte[20];
            Array.Copy(ownerAddressBytes, 1, addressBytes, 0, addressBytes.Length);

            string addressBytesHex = "0x" + addressBytes.ToHex();
            */
            ClaimableRewardsFunction claimableRewards = new ClaimableRewardsFunction { addr_address = address };

            var balanceHandler = web3.Eth.GetContractQueryHandler<ClaimableRewardsFunction>();
          
            var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, claimableRewards);
            double claimableRDNT = balance.DivideToDouble(1000000000000000000);

            return claimableRDNT;
        }

        public async Task<string> StakeTokensAsync(decimal TokensAmount, string Address)
        {
            FunctionABI functionABI = ABITypedRegistry.GetFunctionABI<StakeTokensFunction>();

            try
            {
                StakeTokensFunction stake = new StakeTokensFunction()
                {
                    Amount = (BigInteger)(TokensAmount * 1000000000000000000),
                    Lock = true,
                    OnBehalfOf = Address
                };

                var stakeHandler = web3.Eth.GetContractTransactionHandler<StakeTokensFunction>();
                var estimate = await stakeHandler.EstimateGasAsync(contractAddress, stake);
                stake.Gas = estimate.Value;
                var txid = await stakeHandler.SendRequestAndWaitForReceiptAsync(contractAddress, stake);

                return txid.TransactionHash;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, ex.Message);
                return null;
            }
        }

        [Function("stake")]
        public class StakeTokensFunction : FunctionMessage
        {
            [Parameter("uint256", "amount", 1)]
            public BigInteger Amount { get; set; }

            [Parameter("bool", "lock", 2)]
            public bool Lock { get; set; }

            [Parameter("address", "onBehalfOf", 3)]
            public string OnBehalfOf { get; set; }
        }
    }
}
