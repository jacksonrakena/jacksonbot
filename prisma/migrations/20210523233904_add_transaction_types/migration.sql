/*
  Warnings:

  - Added the required column `type` to the `allTransactions` table without a default value. This is not possible if the table is not empty.

*/
-- CreateEnum
CREATE TYPE "TransactionType" AS ENUM ('CB_OUT_MODERATOR_ADJUSTMENT', 'CB_OUT_BOT_ADMIN_ADJUSTMENT', 'USER_TO_USER', 'CB_IN_USER_PAYMENT');

-- AlterTable
ALTER TABLE "allTransactions" ADD COLUMN     "type" "TransactionType" NOT NULL;
