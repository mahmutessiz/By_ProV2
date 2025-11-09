-- Add Süt and Nakliye Fiyat columns to CASABIT table
ALTER TABLE CASABIT 
ADD SUTFIYATI DECIMAL(18,2) DEFAULT 0,
    NAKFIYATI DECIMAL(18,2) DEFAULT 0;
    
-- Update the columns to have default values for existing records (optional)
UPDATE CASABIT 
SET SUTFIYATI = 0, NAKFIYATI = 0
WHERE SUTFIYATI IS NULL OR NAKFIYATI IS NULL;