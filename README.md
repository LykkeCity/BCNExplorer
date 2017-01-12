# Bcn Exploler

A website explores the blockchain and shows information about blocks, transactions, assets, address balances etc.

#Project structure
* The project contaion 2 main parts:
  *  web application client 
  *  background jobs, which have to collect the blockchain information
  
# How to configure the application?

* Project configuration options stored in azure blob storage as json doc. First of all you have to add connection string to Application settings ("ConnectionString" Key). Then you have to put file named "bcnexplolersettings.json" in "settings" blob storage.


# Structure of "bcnexplolersettings.json" file

```
{
    "NinjaUrl": "",
	"Db": {
		"LogsConnString":"",
		"AssetsConnString":"",
		"SharedStorageConnString":"",
		"SqlConnString":"",
		"AssetBalanceChanges" : {
			"ConnectionString":"",		
			"DbName":"",		
		}
		
	},
	"NinjaIndexerCredentials": {
		"AzureName":"",
		"AzureKey":""								
	},	
	"Authentication": {
		"ClientId":"",
		"ClientSecret":"",
		"PostLogoutRedirectUri":"",
		"Authority":"",			
		"RedirectUrl":""		
	},	
	"Jobs": {
		"IsDebug":false
	},
	Network:"",
	ExplolerUrl:"",
	Secret:""
}
```
* Keys description
  *  NinjaUrl - NbitCoin Ninja (https://github.com/MetacoSA/NBitcoin) instance with blockchain core information
  *  LogsConnString - Connection string that points to the table storage with logging information
  *  AssetsConnString - Connection string that points to the table storage with asset definition information
  *  SqlConnString - Connection string that points to the MS SQL Server database with asset balance changes information
  *  SharedStorageConnString - Connection string that points to the queue which handles slack notifications.
  *  NinjaIndexerCredentials - Connection credentials that points to Azure Table Storage with NbitCoin ninja blockchain info
  *	 AssetBalanceChanges - connection string and db name to mongodb database, which contains data about colored asset changes
  *  Authentication - open id auth options
  * TestNet/MainNet BitcoinNetwork. Write 'mainnet' for MainNet, and 'testnet' for TestNet
  * ExplolerUrl - hosted bcn exploler URL
  * Secret - secret, used in requests to index bcn exploler