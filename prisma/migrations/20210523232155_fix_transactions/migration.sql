/*
  Warnings:

  - You are about to drop the column `accountId` on the `allTransactions` table. All the data in the column will be lost.
  - Added the required column `payeeId` to the `allTransactions` table without a default value. This is not possible if the table is not empty.
  - Added the required column `payerId` to the `allTransactions` table without a default value. This is not possible if the table is not empty.

*/
-- DropForeignKey
ALTER TABLE "allTransactions" DROP CONSTRAINT "allTransactions_accountId_fkey";

-- AlterTable
ALTER TABLE "allTransactions" DROP COLUMN "accountId",
ADD COLUMN     "payeeId" INTEGER NOT NULL,
ADD COLUMN     "payerId" INTEGER NOT NULL;

-- AddForeignKey
ALTER TABLE "allTransactions" ADD FOREIGN KEY ("payeeId") REFERENCES "userAccounts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "allTransactions" ADD FOREIGN KEY ("payerId") REFERENCES "userAccounts"("id") ON DELETE CASCADE ON UPDATE CASCADE;
