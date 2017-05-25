using System;
using System.Collections.Generic;

namespace BCNExplorer.Web.Models
{
    public class OffchainChannelViewModel
    {

        public TransactionViewModel OpenTransaction { get; set; }

        public TransactionViewModel CloseTransaction { get; set; }

        public IEnumerable<OffChainTransactionViewModel> OffChainTransactions { get; set; }

        public static OffchainChannelViewModel Create(TransactionViewModel openTransaction, TransactionViewModel closeTransaction, IEnumerable<OffChainTransactionViewModel> offChainTransactions)
        {
            return new OffchainChannelViewModel
            {
                OpenTransaction = openTransaction,
                CloseTransaction = closeTransaction,
                OffChainTransactions = offChainTransactions
            };
        }
    }

    public class OffChainTransactionViewModel
    {
        public string TransactionId { get; set; }
        public DateTime DateTime { get; set; }

        public string HubAddress { get; set; }

        public string Address1 { get; set; }


        public string Address2 { get; set; }

        public AssetViewModel Asset { get; set; }


        public bool IsRevoked { get; set; }

        public decimal Address1Quantity { get; set; }

        public decimal Address2Quantity { get; set; }


        public int InputCount => 1;
        public int OutputCount => 2;


        public decimal Address1QuantityDiff { get; set; }
        public decimal Address2QuantityDiff { get; set; }
        public decimal TotalQuantity => Address1Quantity + Address2Quantity;

        public static OffChainTransactionViewModel Create(string transactionId, 
            AssetViewModel asset, 
            DateTime dateTime,
            bool isRevoked,
            string hubAddress, 
            string address1, 
            string address2,
            decimal address1Quantity, 
            decimal address2Quantity,
            decimal address1QuantityDiff,
            decimal address2QuantityDiff)
        {
            return new OffChainTransactionViewModel
            {
                TransactionId = transactionId,
                Address1 = address1,
                Address2 =  address2,
                Address1Quantity = address1Quantity,
                Address2Quantity = address2Quantity,
                DateTime = dateTime,
                HubAddress = hubAddress,
                Asset = asset,
                IsRevoked = isRevoked,
                Address1QuantityDiff = address1QuantityDiff,
                Address2QuantityDiff = address2QuantityDiff
            };
        }
    }

    public class OffchainTransactionDetailsViewModel
    {

        public OffchainChannelViewModel Channel { get; set; }

        public OffChainTransactionViewModel Transaction { get; set; }

        public static OffchainTransactionDetailsViewModel Create(OffchainChannelViewModel channel,
            OffChainTransactionViewModel transaction)
        {
            return new OffchainTransactionDetailsViewModel
            {
                Channel = channel,
                Transaction = transaction
            };
        }
    }
}