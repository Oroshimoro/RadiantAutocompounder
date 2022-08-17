Forked from - https://github.com/noescape00/Tron_USDD_AutoCompounder 


# Radiant autocompounder

This bot does the following: 

1.  Check claimable RDNT on https://app.radiant.capital/#/manage-radiant
2. If claimable RDNT > threshold => claim RDNT
3. Stake and lock RDNT tokens



## Prerequisites

Get Alchemy api key here: https://dashboard.alchemyapi.io/

Install .net 6 from here: https://dotnet.microsoft.com/en-us/download/dotnet/6.0

Install Microsoft Visual C++ Redistributable from here: https://docs.microsoft.com/en-US/cpp/windows/latest-supported-vc-redist?view=msvc-170

## How to run

1. Clone repository and build project using `dotnet build --configuration Release`
2. Go to `\bin\Release\net6.0` and edit `appsettings.json`: insert your mnemonic and `AlchemyAPIKey`, also set `ClaimThresholdRDNT` to any desirable value (compounding will happen when claimable RDNT amount is larger than configured threshold)
3. Run `DeFi_Strategies.exe`



Build & setup video here: https://www.youtube.com/watch?v=4C5iuqsIbtg





p.s.

On average it costs 5$ in eth fees to pay for 4 txes to autocompound. 

So probably don't set autocompounding threshold at less than $5. 

I'd suggest using smth like $10-15 for a threshold. 



p.s.s

Contact: @Xudox0 (telegram) - usdd autocompouder author

Me - @vendue_tele
