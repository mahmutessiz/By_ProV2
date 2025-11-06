# By_ProV2 ERP System - Architecture Documentation

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Database Schema](#database-schema)
4. [User Interface Components](#user-interface-components)
5. [Business Logic Components](#business-logic-components)
6. [Authentication and Security](#authentication-and-security)
7. [Data Flow](#data-flow)
8. [Key Features](#key-features)

## Overview

The By_ProV2 ERP system is a comprehensive enterprise resource planning application designed for agricultural and food trading operations, specifically for "Beryem Tarım Ürünleri Gıda Nakliye Ticaret Ltd. Şti." The system manages various business processes including:

- Customer relationship management
- Inventory management
- Dairy intake and processing
- Feed trading operations
- Accounting and tracking
- Parameter management

## System Architecture

### Technology Stack
- **Platform**: .NET 8.0 Windows
- **Framework**: WPF (Windows Presentation Foundation)
- **Database**: Microsoft SQL Server
- **Data Access**: Microsoft.Data.SqlClient
- **UI Language**: Turkish (tr-TR)
- **Authentication**: Custom authentication system

### Architecture Layers
```
┌─────────────────────────┐
│      User Interface     │  (XAML + Code-behind)
├─────────────────────────┤
│    Business Logic       │  (Services, Helpers)
├─────────────────────────┤
│      Data Access        │  (Repositories, Models)
├─────────────────────────┤
│      Database           │  (SQL Server)
└─────────────────────────┘
```

### Core Components

#### 1. User Interface Layer
- WPF Windows and UserControls
- XAML-based layouts
- Data binding with MVVM patterns
- Turkish localization support

#### 2. Business Logic Layer
- Service classes for business operations
- Helper classes for common operations
- Parameter management
- Authentication services

#### 3. Data Access Layer
- Repository pattern implementation
- Model classes (Data Transfer Objects)
- Database connection management

#### 4. Database Layer
- SQL Server database
- Transaction support for data consistency
- User tracking fields for audit trails

## Database Schema

### Core Tables

#### 1. Cari (Customer/Supplier)
- **Purpose**: Stores customer and supplier information
- **Fields**:
  - CariId (Primary Key, Identity)
  - CariKod, CariAdi (Customer code and name)
  - Tipi (Type: Customer/Supplier)
  - Contact information (address, phone, tax info)
  - Discount rates (ISK1-4, KKISK1-4, NAKISK)
  - Vehicle info (plaka, driver)
  - User tracking fields: CreatedBy, ModifiedBy, CreatedAt, ModifiedAt

#### 2. SutKayit (Dairy Record)
- **Purpose**: Tracks dairy intake, depot dispatch, and direct dispatch operations
- **Fields**:
  - SutKayitId (Primary Key, Identity)
  - BelgeNo (Document number)
  - Tarih, IslemTuru (Date, Operation Type)
  - Supplier/Client IDs and info
  - Dairy analysis: Yag (fat), Protein, Laktoz, pH, etc.
  - Price and quantity fields
  - User tracking fields: CreatedBy, ModifiedBy, CreatedAt, ModifiedAt

#### 3. STOKSABITKART (Inventory Catalog)
- **Purpose**: Master inventory items catalog
- **Fields**:
  - STOKID (Primary Key, Identity)
  - STOKKODU, STOKADI (Code, Name)
  - Unit, weight, protein, energy, moisture
  - Barcode, properties, origin
  - User tracking fields

#### 4. STOKSABITFIYAT (Price Catalog)
- **Purpose**: Price management for inventory items
- **Fields**:
  - FIYATID (Primary Key, Identity)
  - STOKID (Foreign Key)
  - Price list information
  - Multiple purchase prices for different payment terms
  - Tax rate, currency
  - User tracking fields

#### 5. STOKSABITBELGE (Document Catalog)
- **Purpose**: Document management for inventory items
- **Fields**:
  - BELGEID (Primary Key, Identity)
  - STOKID (Foreign Key)
  - BELGETIPI (Document type)
  - DOSYAYOLU (File path)
  - EKLEMETARIHI (Add date)
  - User tracking fields

#### 6. STOKSABITHAREKET (Inventory Movement)
- **Purpose**: Tracks inventory movements
- **Fields**:
  - HAREKETID (Primary Key, Identity)
  - STOKID (Foreign Key)
  - HAREKETTURU (Movement type: Entry/Exit)
  - Quantity, unit, depot
  - ISLEMTARIHI (Process date)
  - User tracking fields

#### 7. STOKSABITTED (Supplier Catalog)
- **Purpose**: Supplier catalog
- **Fields**:
  - TEDARIKCIID (Primary Key, Identity)
  - TEDARIKCIADI (Supplier name)
  - User tracking fields

#### 8. SiparisMaster (Order Master)
- **Purpose**: Master table for orders and proformas
- **Fields**:
  - SiparisID (Primary Key, Identity)
  - Document codes and numbers
  - Dates (order, shipment)
  - Customer information
  - Payment and delivery terms
  - User tracking fields

#### 9. SiparisKalemAlis/SiparisKalemSatis (Order Lines)
- **Purpose**: Order line items
- **Fields**:
  - KalemID (Primary Key, Identity)
  - SiparisID (Foreign Key)
  - Stock information, quantity, price
  - Discounts and total amounts
  - User tracking fields

#### 10. Users (Authentication)
- **Purpose**: User authentication and authorization
- **Fields**:
  - Id (Primary Key, Identity)
  - Username (Unique), PasswordHash
  - Email, FullName, Role
  - IsActive, CreatedAt, LastLoginAt
  - CreatedBy, ModifiedBy, CreatedAt, ModifiedAt

#### 11. Numarator (Document Numbering)
- **Purpose**: Sequential document numbering
- **Fields**:
  - Yil, Tip (Year, Type)
  - SonNumara (Last number)
  - Primary key: Yil, Tip

#### 12. DepoStok (Depot Stock)
- **Purpose**: Depot stock tracking
- **Fields**:
  - DepoStokId (Primary Key, Identity)
  - Tarih, TedarikciId
  - Quantity, fat, protein, TKM
  - User tracking fields

#### 13. Parametreler (Parameters)
- **Purpose**: System parameters for operations
- **Fields**:
  - ParametreId (Primary Key, Identity)
  - YagKesintiParametresi (Fat cutoff parameter)
  - ProteinParametresi (Protein parameter)
  - DizemBasiTl (Price per dizem)
  - CreatedAt

## User Interface Components

### 1. MainWindow.xaml
- **Purpose**: Main application navigation
- **Features**:
  - Menu and navigation buttons
  - Connection status indicator
  - User status display
  - Central content area for sub-windows

### 2. Core Business Windows
- **CariKayitWindow**: Customer/Supplier registration and management
- **SutAlimFormu**: Dairy intake operations with different modes
- **StokKayitWindow**: Inventory item management
- **SiparisFormu**: Order processing system
- **ParametrelerWindow**: Parameter management interface

### 3. Supporting Windows
- **CariListesiWindow**: Customer list selection
- **StokListeWindow**: Inventory list
- **SutDepoSevkFormu**: Depot dispatch operations
- **SutDirekSevkFormu**: Direct dispatch operations
- **BelgeSorgulama**: Document querying
- **AuditTrailWindow**: Audit trail viewer

### 4. Authentication Windows
- **LoginWindow**: User login interface
- **UserManagementWindow**: Admin user management
- **FirstTimeSetupWindow**: Initial admin setup

### 5. Specialized Windows
- **SutRaporlari**: Dairy reports
- **GunlukSutAlimPreview**: Daily dairy intake preview
- **EskiSiparisFormu**: Legacy order processing

## Business Logic Components

### 1. Repository Pattern
- **SutRepository**: Handles dairy records
- **CariRepository**: Manages customer data
- **ParameterRepository**: Parameter management
- **UserRepository**: User management
- **DepoStokRepository**: Depot stock operations

### 2. Business Services
- **AuthenticationService**: User authentication and session management
- **DocumentNumberGenerator**: Sequential document numbering

### 3. Helper Classes
- **DatabaseInitializer**: Database creation and initialization
- **DatabaseHelper**: Database utility functions
- **CustomFontResolver**: PDF font management

### 4. Business Logic Components
- **DepoyaAlimIslemi**: Dairy intake business logic
- **DepodanSevkIslemi**: Depot dispatch business logic
- **DirektSevkIslemi**: Direct dispatch business logic

## Authentication and Security

### Security Features
- **Role-based Access Control**: Admin/User roles
- **User Tracking**: All operations track creating/modifying users
- **Session Management**: Current user context throughout application
- **Secure Password Storage**: Hashed passwords with salt

### Database Security
- User tracking fields in all major tables
- CreatedBy/ModifiedBy references to Users table
- CreatedAt/ModifiedAt timestamps for audit trails

## Data Flow

### Typical Operation Flow
1. **User Authentication**
   - LoginWindow → AuthenticationService → Session Context

2. **Data Entry**
   - UI Component → Validation → Repository → Database (with transaction)

3. **Data Retrieval**
   - UI Request → Repository → Database → Model Objects → UI Binding

4. **Data Update**
   - UI Component → Validation → Repository → Database Update Transaction

### Transaction Management
- Multi-table operations use database transactions
- Stock movements and dairy records use coordinated operations
- Rollback on any failure in multi-step operations

### Document Numbering Flow
- DocumentNumberGenerator generates sequential numbers
- Numarator table maintains sequence state
- Different document types have independent sequences

## Key Features

### 1. Multi-Operation Dairy Management
- Dairy intake from suppliers
- Depot dispatch to customers
- Direct dispatch from suppliers to customers
- Complete analysis tracking (fat, protein, pH, etc.)

### 2. Comprehensive Customer Management
- Customer and supplier tracking
- Multiple discount types
- Vehicle and driver information
- Tax and contact details

### 3. Flexible Inventory Management
- Multiple price lists for different payment terms
- Document attachment support
- Depot movement tracking
- Detailed product specifications

### 4. Robust Order Processing
- Order/proforma management
- Multiple line items
- Discount calculations
- Payment term flexibility

### 5. Parameter Management
- System-wide parameter control
- Change tracking
- Update rather than insert functionality

### 6. Audit Trail System
- User action tracking
- Complete operation history
- Administrative oversight capabilities

### 7. Multi-Language Support
- Turkish localization
- Date/time formatting
- Currency handling

## System Integration Points

### External Systems
- Microsoft SQL Server database
- PDF generation for reports
- File system for document storage

### Internal Dependencies
- Database connection with connection string management
- User authentication context
- Document numbering system
- Audit trail functionality

## Development Patterns Used

### 1. Repository Pattern
- Separation of data access logic
- Database abstraction
- Testability improvements

### 2. Singleton Services
- AuthenticationService maintains global user context
- Consistent session across application

### 3. MVVM Pattern (Partially)
- Data binding in XAML
- Command patterns in UI
- Model separation

### 4. Transaction Management
- Database transaction support
- Multi-table consistency
- Rollback capabilities

This architecture provides a comprehensive framework for ERP operations with strong security, audit capabilities, and modular design patterns that allow for future expansion and maintenance.