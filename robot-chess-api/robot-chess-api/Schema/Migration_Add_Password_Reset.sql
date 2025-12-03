-- Migration: Add Password Reset columns to app_users table
-- Run this in Supabase SQL Editor

-- Add password_reset_token column
ALTER TABLE public.app_users
ADD COLUMN IF NOT EXISTS password_reset_token TEXT;

-- Add password_reset_token_expiry column
ALTER TABLE public.app_users
ADD COLUMN IF NOT EXISTS password_reset_token_expiry TIMESTAMPTZ;

-- Add comment to the columns
COMMENT ON COLUMN public.app_users.password_reset_token IS 'Token for password reset verification';
COMMENT ON COLUMN public.app_users.password_reset_token_expiry IS 'Expiry time for password reset token';
