/*
  Warnings:

  - Added the required column `amount` to the `allTransactions` table without a default value. This is not possible if the table is not empty.

*/
-- AlterTable
ALTER TABLE "allTransactions" ADD COLUMN     "amount" INTEGER NOT NULL;
