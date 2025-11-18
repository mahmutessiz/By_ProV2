### Table Documentation

This document outlines the purpose and behavior of the primary tables within the application.

---

## System & Configuration

These tables manage core system functionalities like user access, application-wide settings, and document numbering.

#### `Users`

*   **Purpose:** Stores user account information for authentication and authorization within the application, including usernames, hashed passwords, roles, and status.

*   **Affected by:**
    *   **Creation:** New users are created via `UserRepository.CreateUser`. This is triggered from the "User Management" window (`UserManagementWindow.xaml`) and also during the `FirstTimeSetupWindow.xaml` if no users exist.
    *   **Read:** `UserRepository.GetUserByUsername` is used during the login process in the "Login" window (`LoginWindow.xaml`) to authenticate the user. `GetAllUsers` is used to populate the list in the "User Management" window.
    *   **Update:** `UserRepository.UpdatePassword` is used to change a user's password. `UpdateLastLogin` is called after a successful login. Other user details (like role or active status) are updated from the "User Management" window.
    *   **Deletion (Deactivation):** Users are typically not deleted but are made inactive by setting the `IsActive` flag to `false` from the "User Management" window.

---

#### `Parametreler`

*   **Purpose:** Stores application-wide parameters, specifically for milk quality calculations (e.g., freezing point reference, fat deduction rates). It is designed to hold a single row of the most current parameters, which is updated rather than having new rows added.

*   **Affected by:**
    *   **Creation/Update:** The `ParameterRepository.KaydetParametre` method is used to save parameters. It checks if any record exists; if so, it updates the single row; if not, it inserts a new one. This is triggered from the "Parametreler" window (`ParametrelerWindow.xaml`).
    *   **Read:** The `ParameterRepository.GetLatestParametreler` method is used to retrieve the most recent parameters. This is called from the "Süt Alım Formu" (`SutAlimFormu.xaml`) to perform milk quality and net quantity calculations.

---

#### `Numarator`

*   **Purpose:** This table is designed to store the last used sequential number for different document types within a given year, enabling the generation of unique, incremental document numbers (e.g., for sales orders, purchase orders, etc.). It contains fields for `Yil` (Year), `Tip` (Type, e.g., "S" for Sales, "P" for Purchase), and `SonNumara` (Last Number).

*   **Affected by:**
    *   **Read/Update/Create:** The `NumaratorService.GetNextNumberFromDb` method is responsible for all interactions. It reads the `SonNumara` for a specific year and type. If a record exists, it increments `SonNumara` and updates the record. If no record exists, it inserts a new record with `SonNumara` initialized to 1. This process uses database locks (`UPDLOCK, ROWLOCK`) to ensure thread-safe and atomic generation of unique numbers.
    *   **Note:** While `NumaratorService` is designed for sequential number generation, the `Helpers/DocumentNumberGenerator.cs` currently uses a random number generation approach for documents like "SUT-YYYY-XXXXXXX". This suggests that the `Numarator` table might be intended for future sequential numbering implementations or for other document types not yet integrated with the random generator.

---

## Core Entities

These tables represent the fundamental business entities like customers, suppliers, and products.

### Current Accounts (Cari)

#### `CASABIT`

*   **Purpose:** This is the main and most detailed table for "Cari" accounts (representing customers, suppliers, and other business entities). It stores a comprehensive set of information, including contact details, tax information, various discount rates, and logistical data like license plates and driver names. It is the primary source of truth for account management in the application's user interface.

*   **Affected by:**
    *   **Creation:** A new, detailed account record is inserted when a user saves a new account in the "Cari Kayıt" window (`CariKayitWindow.xaml`).
    *   **Read:** Records are read from this table to populate the list in the "Cari Listesi" window (`CariListesiWindow.xaml`). This window is used for searching and selecting accounts in various parts of the application, such as the "Süt Alım Formu" (Milk Purchase Form) and the "Cari Kayıt" window itself.
    *   **Update:** An existing record is updated when a user modifies and saves an account in the "Cari Kayıt" window.
    *   **Deletion:** A record is deleted when a user deletes an account from the "Cari Kayıt" window.

---

#### `Cari`

*   **Purpose:** This is a simpler, auxiliary table for "Cari" accounts, containing only the essential fields: `CariId`, `CariKod`, `CariAdi`, and `Tipi`.

*   **Affected by:**
    *   **Creation/Read:** The `DataAccess/CariRepository.cs` file contains a `GetOrCreateCari` method that can read or insert records into this table.
    *   **Note:** The direct usage of this table and its repository is not apparent in the main UI workflows analyzed (like `CariKayitWindow` or `SutAlimFormu`), which interface directly with the more detailed `CASABIT` table. The `Cari` table may be used by other background processes or could be part of a legacy feature.

---

### General Stock Items (Stok)

#### `STOKSABITKART`

*   **Purpose:** This is the master table for general stock items. It stores the main information for each stock article, such as stock code, name, unit, and VAT rate. It serves as the parent table for `STOKSABITFIYAT` and `STOKSABITBELGE`.

*   **Affected by:**
    *   **Creation:** A new record is inserted when a user saves a new stock item in the "Stok Kayıt" window (`StokKayitWindow.xaml`).
    *   **Update:** An existing record is updated when a user modifies and saves a stock item in the "Stok Kayıt" window.
    *   **Deletion:** A record is deleted when a user deletes a stock item from the "Stok Kayıt" window.

---

#### `STOKSABITFIYAT`

*   **Purpose:** This table stores pricing information for the general stock items defined in `STOKSABITKART`. It is linked to the parent table via the `STOKID` foreign key.

*   **Affected by:**
    *   **Creation/Update:** Records are inserted or updated whenever the pricing information for a stock item is saved in the "Stok Kayıt" window (`StokKayitWindow.xaml`).
    *   **Deletion:** Records are deleted when the corresponding parent stock item is deleted from `STOKSABITKART`.

---

#### `STOKSABITBELGE`

*   **Purpose:** This table stores file paths for documents associated with a general stock item (e.g., images, technical specifications). It is linked to `STOKSABITKART` via the `STOKID` foreign key.

*   **Affected by:**
    *   **Creation/Update:** Records are inserted or updated when a document path is added or changed for a stock item in the "Stok Kayıt" window (`StokKayitWindow.xaml`).
    *   **Deletion:** Records are deleted when the corresponding parent stock item is deleted from `STOKSABITKART`.

---

#### `STOKSABITTED`

*   **Purpose:** This is a lookup table that stores information about suppliers (`Tedarikçi`).

*   **Affected by:**
    *   **Creation:** New supplier records are added through the "Tedarikçi Ekle" window (`TedarikciEkleWindow.xaml`).
    *   **Read:** This table is read to populate the supplier dropdown/combobox in the "Stok Kayıt" window (`StokKayitWindow.xaml`).

---

## Transactional Tables

These tables store records of business operations like orders, purchases, and inventory changes.

### Orders (Sipariş)

#### `SiparisMaster`

*   **Purpose:** This is the main table for storing order (Sipariş) details. It holds header-level information about an order, including order numbers (`SiparisNo`, `ProformaNo`), dates, associated customer/supplier (`Cari`) information (both for purchase and sales sides), delivery details, payment terms, and various descriptive fields. It acts as the parent table for `SiparisKalemAlis` and `SiparisKalemSatis`.

*   **Affected by:**
    *   **Creation:** A new record is inserted into `SiparisMaster` when `MainViewModel.SiparisiKaydetAsync()` is called (triggered by the "Kaydet" button in `SiparisFormu.xaml.cs`). A new `SiparisID` is generated upon insertion.
    *   **Read:** Records are read from `SiparisMaster` when `MainViewModel.SiparisiYukleAsync(string belgeKodu)` is called (e.g., when loading an existing order for viewing or editing).
    *   **Update:** An existing record is updated in `SiparisMaster` when `MainViewModel.SiparisiGuncelleAsync(int siparisID)` is called. This method first deletes all associated `SiparisKalemAlis` and `SiparisKalemSatis` records, then updates the master record, and finally re-inserts the item lines.
    *   **Deletion:** A record is deleted from `SiparisMaster` when `MainViewModel.SiparisiSilAsync(int siparisID)` is called. This method also deletes all associated `SiparisKalemAlis` and `SiparisKalemSatis` records.

---

#### `SiparisKalemAlis`

*   **Purpose:** This table stores the individual line items (kalemler) for the *purchase* side of an order. Each record represents a specific stock item being purchased as part of a `SiparisMaster` order. It is linked to `SiparisMaster` by `SiparisID`. It includes details like stock code, name, quantity, unit price, discounts, and total amount.

*   **Affected by:**
    *   **Creation:** New records are inserted into `SiparisKalemAlis` when `MainViewModel.SiparisiKaydetAsync()` is called. These records correspond to the items in the `AlisKalemListesi` of the `MainViewModel`.
    *   **Read:** Records are read from `SiparisKalemAlis` when `MainViewModel.SiparisiYukleAsync(string belgeKodu)` is called, populating the `AlisKalemListesi`.
    *   **Update:** When `MainViewModel.SiparisiGuncelleAsync(int siparisID)` is called, all existing `SiparisKalemAlis` records for that `SiparisID` are first deleted, and then new records (from the updated `AlisKalemListesi`) are inserted.
    *   **Deletion:** Records are deleted from `SiparisKalemAlis` when `MainViewModel.SiparisiSilAsync(int siparisID)` is called, or as part of the update process in `SiparisiGuncelleAsync`.

---

#### `SiparisKalemSatis`

*   **Purpose:** This table stores the individual line items (kalemler) for the *sales* side of an order. Each record represents a specific stock item being sold as part of a `SiparisMaster` order. It is linked to `SiparisMaster` by `SiparisID`. It includes details like stock code, name, quantity, unit price, discounts, and total amount.

*   **Affected by:**
    *   **Creation:** New records are inserted into `SiparisKalemSatis` when `MainViewModel.SiparisiKaydetAsync()` is called. These records correspond to the items in the `SatisKalemListesi` of the `MainViewModel`.
    *   **Read:** Records are read from `SiparisKalemSatis` when `MainViewModel.SiparisiYukleAsync(string belgeKodu)` is called, populating the `SatisKalemListesi`.
    *   **Update:** When `MainViewModel.SiparisiGuncelleAsync(int siparisID)` is called, all existing `SiparisKalemSatis` records for that `SiparisID` are first deleted, and then new records (from the updated `SatisKalemListesi`) are inserted.
    *   **Deletion:** Records are deleted from `SiparisKalemSatis` when `MainViewModel.SiparisiSilAsync(int siparisID)` is called, or as part of the update process in `SiparisiGuncelleAsync`.

---

### Milk Transactions & Inventory

#### `SutKayit`

*   **Purpose:** This table stores detailed records of milk purchase and transaction entries. Each record represents a specific milk transaction, including quality parameters, quantities, and associated supplier/customer information.

*   **Affected by:**
    *   **Creation:** New milk transaction records are inserted when a user saves entries in the "Süt Alım Formu" (`SutAlimFormu.xaml`). This is handled by the `SutRepository.KaydetSutKaydi` method.
    *   **Read:** Records are retrieved for display and editing in the "Süt Alım Formu" via methods like `SutRepository.GetSutKaydiById` and `SutRepository.GetSutKayitlariByBelgeNo`.
    *   **Update:** Existing milk transaction records are updated when a user modifies and saves entries in the "Süt Alım Formu". This is handled by the `SutRepository.GuncelleSutKaydi` method.
    *   **Deletion:** Records can be deleted from this table via the `SutRepository.SilSutKaydi` method, typically triggered by user actions in the "Süt Alım Formu".

---

#### `DepoStok`

*   **Purpose:** This table specifically tracks the inventory levels and movements of milk within the depot. It is distinct from `STOKSABITHAREKET`, which handles general stock items. `DepoStok` records reflect changes in milk quantity based on transactions.

*   **Affected by:**
    *   **Creation/Update:** Inventory changes are recorded or updated when milk transactions are saved or modified in the "Süt Alım Formu" (`SutAlimFormu.xaml`). These operations are managed indirectly through `SutEnvanteriService` which utilizes `DepoStokRepository` methods like `KaydetStokHareketi` and `GuncelleStokHareketi`.
    *   **Read:** Current inventory levels are read to display in the "Süt Alım Formu" via `SutEnvanteriService.GetEnvanterByTarih`.
    *   **Reversion:** When a milk transaction is deleted or reverted, the corresponding inventory adjustment in `DepoStok` is handled by `SutEnvanteriService.RevertTransaction`.

---

#### `SutEnvanteri`

*   **Purpose:** Stores daily summary records of milk inventory. It tracks the opening balance (`DevirSut`), total milk received (`GunlukAlinanSut`), total milk sold/dispatched (`GunlukSatilanSut`), and the closing balance (`KalanSut`) for a specific date.

*   **Affected by:**
    *   **Creation/Update:** The `SutEnvanteriService` automatically calculates and updates the daily inventory record based on transactions from the "Süt Alım Formu". It calls `SutEnvanteriRepository` methods (`KaydetEnvanter`, `GuncelleEnvanter`) to persist these summaries.
    *   **Read:** Records are read by `SutEnvanteriRepository.GetAllEnvanter` and `GetEnvanterByTarih` to be displayed in the "Süt Envanteri" window (`SutEnvanteriWindow.xaml`).
    *   **Deletion:** Records can be deleted via `SutEnvanteriRepository.SilEnvanter`.

---

### General Stock Transactions

#### `STOKSABITHAREKET`

*   **Purpose:** This table tracks the inventory movements (transactions) for **general stock items only**. It is explicitly separate from the milk inventory system, which uses a different table (`DepoStok`).

*   **Affected by:**
    *   **Creation (Initial Record):** When a new general stock item is created in the "Stok Kayıt" window (`StokKayitWindow.xaml`), an initial transaction record with zero quantity is inserted into this table to establish a starting point for the inventory.
    *   **Further Movements:** Subsequent stock movements (e.g., purchases, sales, adjustments) for these general items would be recorded here from other parts of the application, such as a dedicated purchasing or sales form (which is distinct from the milk-handling forms).