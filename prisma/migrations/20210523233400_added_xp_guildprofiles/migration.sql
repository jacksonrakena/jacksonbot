-- CreateEnum
CREATE TYPE "XpChangeType" AS ENUM ('MANUAL_ADJUSTMENT', 'TYPING', 'DAILY', 'EVENT');

-- AlterTable
ALTER TABLE "userAccounts" ADD COLUMN     "xpBalance" INTEGER NOT NULL DEFAULT 0;

-- CreateTable
CREATE TABLE "xpChangeEvents" (
    "id" SERIAL NOT NULL,
    "amount" INTEGER NOT NULL,
    "type" "XpChangeType" NOT NULL,
    "userId" INTEGER NOT NULL,

    PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "guildProfiles" (
    "id" TEXT NOT NULL,
    "starboardChannelId" TEXT NOT NULL,
    "starboardThreshold" INTEGER NOT NULL DEFAULT 3,

    PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "GuildPrefix" (
    "id" SERIAL NOT NULL,
    "guildId" TEXT NOT NULL,
    "prefix" TEXT NOT NULL,

    PRIMARY KEY ("id")
);

-- AddForeignKey
ALTER TABLE "xpChangeEvents" ADD FOREIGN KEY ("userId") REFERENCES "userAccounts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "GuildPrefix" ADD FOREIGN KEY ("guildId") REFERENCES "guildProfiles"("id") ON DELETE CASCADE ON UPDATE CASCADE;
