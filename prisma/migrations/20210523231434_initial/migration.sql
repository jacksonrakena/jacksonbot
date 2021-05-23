-- CreateTable
CREATE TABLE "starredMessages" (
    "messageId" TEXT NOT NULL,
    "channelId" TEXT NOT NULL,
    "authorId" TEXT NOT NULL,
    "starCount" INTEGER NOT NULL,
    "displayMessageId" TEXT NOT NULL,
    "displayChannelId" TEXT NOT NULL,

    PRIMARY KEY ("messageId")
);

-- CreateTable
CREATE TABLE "allTransactions" (
    "id" SERIAL NOT NULL,
    "accountId" INTEGER NOT NULL,
    "date" TIMESTAMP(3) NOT NULL,

    PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "userAccounts" (
    "id" SERIAL NOT NULL,
    "userId" TEXT NOT NULL,
    "guildId" TEXT NOT NULL,

    PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "userAccounts.userId_guildId_unique" ON "userAccounts"("userId", "guildId");

-- AddForeignKey
ALTER TABLE "allTransactions" ADD FOREIGN KEY ("accountId") REFERENCES "userAccounts"("id") ON DELETE CASCADE ON UPDATE CASCADE;
