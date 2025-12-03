-- Add email verification columns to app_users table
-- Run this script to update the database schema

ALTER TABLE app_users 
ADD COLUMN IF NOT EXISTS email_verified BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN IF NOT EXISTS email_verification_token TEXT,
ADD COLUMN IF NOT EXISTS email_verification_token_expiry TIMESTAMP WITH TIME ZONE;

-- Add index for faster token lookup
CREATE INDEX IF NOT EXISTS idx_app_users_verification_token 
ON app_users(email_verification_token) 
WHERE email_verification_token IS NOT NULL;

-- Add comment to document the new columns
COMMENT ON COLUMN app_users.email_verified IS 'Indicates if the user has verified their email address';
COMMENT ON COLUMN app_users.email_verification_token IS 'Token used for email verification';
COMMENT ON COLUMN app_users.email_verification_token_expiry IS 'Expiration time for the email verification token';
