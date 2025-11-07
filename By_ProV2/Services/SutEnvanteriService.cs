using System;
using By_ProV2.DataAccess;
using By_ProV2.Models;

namespace By_ProV2.Services
{
    public class SutEnvanteriService
    {
        private readonly SutEnvanteriRepository _envanterRepository;

        public SutEnvanteriService()
        {
            _envanterRepository = new SutEnvanteriRepository();
        }

        /// <summary>
        /// Updates inventory when a milk transaction occurs
        /// </summary>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <param name="transactionType">Type of transaction: "Depoya Alım", "Depodan Sevk", "Direkt Sevk"</param>
        /// <param name="miktar">Amount of milk in the transaction</param>
        public void UpdateInventoryForTransaction(DateTime transactionDate, string transactionType, decimal miktar)
        {
            // Get existing inventory record for the day, or create a new one
            var envanter = _envanterRepository.GetEnvanterByTarih(transactionDate);
            
            if (envanter == null)
            {
                // Create a new inventory record for this date
                envanter = new SutEnvanteri
                {
                    Tarih = transactionDate.Date,
                    DevirSut = GetPreviousDayClosingBalance(transactionDate),
                    GunlukAlinanSut = 0,
                    GunlukSatilanSut = 0,
                    Aciklama = $"Otomatik envanter girişi - {transactionType}"
                };
            }

            // Update the appropriate field based on transaction type
            switch (transactionType)
            {
                case "Depoya Alım": // Milk received/purchased
                    envanter.GunlukAlinanSut += miktar;
                    break;
                
                case "Depodan Sevk": // Milk sold from warehouse
                case "Direkt Sevk": // Direct shipment/sale
                    envanter.GunlukSatilanSut += miktar;
                    break;
                
                default:
                    // For other transaction types, no inventory update needed
                    return;
            }

            // Recalculate remaining milk
            envanter.KalanSut = envanter.DevirSut + envanter.GunlukAlinanSut - envanter.GunlukSatilanSut;

            // Save the inventory record
            if (envanter.Id == 0) // New record
            {
                _envanterRepository.KaydetEnvanter(envanter);
            }
            else // Update existing record
            {
                _envanterRepository.GuncelleEnvanter(envanter);
            }
        }

        /// <summary>
        /// Updates inventory when a milk transaction is updated (edited)
        /// </summary>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <param name="previousTransactionType">Previous type of transaction</param>
        /// <param name="newTransactionType">New type of transaction</param>
        /// <param name="previousMiktar">Previous amount</param>
        /// <param name="newMiktar">New amount</param>
        public void UpdateInventoryForTransactionChange(DateTime transactionDate, string previousTransactionType, string newTransactionType, decimal previousMiktar, decimal newMiktar)
        {
            // First, revert the previous transaction
            RevertTransaction(transactionDate, previousTransactionType, previousMiktar);
            
            // Then apply the new transaction
            UpdateInventoryForTransaction(transactionDate, newTransactionType, newMiktar);
        }

        /// <summary>
        /// Updates inventory when a milk transaction is deleted
        /// </summary>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <param name="transactionType">Type of transaction</param>
        /// <param name="miktar">Amount of milk in the transaction</param>
        public void RevertTransaction(DateTime transactionDate, string transactionType, decimal miktar)
        {
            var envanter = _envanterRepository.GetEnvanterByTarih(transactionDate);
            if (envanter == null)
            {
                // If no inventory record exists for this date, nothing to revert
                return;
            }

            // Adjust the appropriate field based on transaction type
            switch (transactionType)
            {
                case "Depoya Alım": // Milk received/purchased
                    envanter.GunlukAlinanSut -= miktar;
                    break;
                
                case "Depodan Sevk": // Milk sold from warehouse
                case "Direkt Sevk": // Direct shipment/sale
                    envanter.GunlukSatilanSut -= miktar;
                    break;
                
                default:
                    // For other transaction types, no inventory update needed
                    return;
            }

            // Ensure no negative values
            if (envanter.GunlukAlinanSut < 0) envanter.GunlukAlinanSut = 0;
            if (envanter.GunlukSatilanSut < 0) envanter.GunlukSatilanSut = 0;

            // Recalculate remaining milk
            envanter.KalanSut = envanter.DevirSut + envanter.GunlukAlinanSut - envanter.GunlukSatilanSut;

            // Save the updated inventory record
            _envanterRepository.GuncelleEnvanter(envanter);
        }

        /// <summary>
        /// Gets the closing balance from the previous day to use as opening balance (devir) for today
        /// </summary>
        /// <param name="date">Date to get previous day's closing balance for</param>
        /// <returns>Closing balance from previous day, or 0 if not available</returns>
        private decimal GetPreviousDayClosingBalance(DateTime date)
        {
            DateTime previousDay = date.Date.AddDays(-1);
            var previousDayEnvanter = _envanterRepository.GetEnvanterByTarih(previousDay);
            
            if (previousDayEnvanter != null)
            {
                return previousDayEnvanter.KalanSut; // Use the remaining milk from previous day as devir
            }
            
            return 0; // If no previous day record, start with 0
        }
    }
}