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

        /// <param name="transactionDate">Date of the transaction</param>
        /// <param name="transactionType">Type of transaction: "Depoya Alım", "Depodan Sevk", "Direkt Sevk"</param>
        /// <param name="miktar">Amount of milk in the transaction</param>
        public void UpdateInventoryForTransaction(DateTime transactionDate, string transactionType, decimal miktar)
        {
            // First handle day change logic to ensure proper records exist
            if (transactionDate.Date == DateTime.Today.Date)
            {
                HandleDayChange(transactionDate);
            }

            // Get existing inventory record for the day
            var envanter = _envanterRepository.GetEnvanterByTarih(transactionDate);
            
            // If no inventory record exists for this date even after handling day change, 
            // we should not create a new record for past dates - only allow updates to existing records
            if (envanter == null)
            {
                // For today's date, allow creating if it doesn't exist after the day change handling
                if (transactionDate.Date == DateTime.Today.Date)
                {
                    // Create new record for today if still doesn't exist after day change
                    envanter = new SutEnvanteri
                    {
                        Tarih = transactionDate.Date,
                        DevirSut = GetPreviousDayClosingBalance(transactionDate),
                        GunlukAlinanSut = 0,
                        GunlukSatilanSut = 0,
                        KalanSut = GetPreviousDayClosingBalance(transactionDate),
                        Aciklama = $"Otomatik envanter girişi - {transactionType}"
                    };
                    _envanterRepository.KaydetEnvanter(envanter);
                }
                else
                {
                    // For past dates, don't allow creating new records - only update existing ones
                    return; // Cannot update inventory for a date that doesn't have an existing record
                }
            }

            // Update the appropriate field based on transaction type
            switch (transactionType)
            {
                case "Depoya Alım": // Milk received/purchased
                    envanter.GunlukAlinanSut += miktar;
                    break;

                case "Depodan Sevk": // Milk sold from warehouse
                    envanter.GunlukSatilanSut += miktar;
                    break;

                default:
                    // For other transaction types, no inventory update needed
                    return;
            }

            // Recalculate remaining milk
            envanter.KalanSut = envanter.DevirSut + envanter.GunlukAlinanSut - envanter.GunlukSatilanSut;

            // Save the inventory record
            _envanterRepository.GuncelleEnvanter(envanter);
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
            // First handle day change logic to ensure proper records exist
            if (transactionDate.Date == DateTime.Today.Date)
            {
                HandleDayChange(transactionDate);
            }

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
        /// Handles day change logic by checking if a new day has started and updating inventory accordingly
        /// Adds previous day's remaining milk to today's transfer milk and resets other fields
        /// </summary>
        public void HandleDayChange(DateTime currentDate = default)
        {
            if (currentDate == default)
                currentDate = DateTime.Today;

            var mostRecentEnvanter = _envanterRepository.GetMostRecentEnvanter();

            // If there are no records at all, create the very first one for the current date.
            if (mostRecentEnvanter == null)
            {
                var firstRecord = new SutEnvanteri
                {
                    Tarih = currentDate.Date,
                    DevirSut = 0,
                    GunlukAlinanSut = 0,
                    GunlukSatilanSut = 0,
                    KalanSut = 0,
                    Aciklama = "İlk envanter kaydı"
                };
                _envanterRepository.KaydetEnvanter(firstRecord);
                return; // Exit after creating the first record.
            }

            DateTime dateToProcess = mostRecentEnvanter.Tarih.Date;
            decimal lastBalance = mostRecentEnvanter.KalanSut;

            // Loop from the day AFTER the most recent record up to the current date.
            while (dateToProcess < currentDate.Date)
            {
                dateToProcess = dateToProcess.AddDays(1);

                // Check if a record for this new day already exists.
                var existingRecord = _envanterRepository.GetEnvanterByTarih(dateToProcess);
                if (existingRecord == null)
                {
                    // If it doesn't exist, create it using the last known balance.
                    var newDayRecord = new SutEnvanteri
                    {
                        Tarih = dateToProcess,
                        DevirSut = lastBalance,
                        GunlukAlinanSut = 0,
                        GunlukSatilanSut = 0,
                        KalanSut = lastBalance, // At the start of the day, Kalan = Devir
                        Aciklama = "Günlük gün başı envanteri - Oto"
                    };
                    _envanterRepository.KaydetEnvanter(newDayRecord);
                    // Update lastBalance for the next potential iteration
                    lastBalance = newDayRecord.KalanSut;
                }
                else
                {
                    // If it exists, update our 'lastBalance' to its value in case we need to loop again.
                    lastBalance = existingRecord.KalanSut;
                }
            }
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

        public SutEnvanteri GetEnvanterByTarih(DateTime tarih)
        {
            return _envanterRepository.GetEnvanterByTarih(tarih);
        }
    }
}