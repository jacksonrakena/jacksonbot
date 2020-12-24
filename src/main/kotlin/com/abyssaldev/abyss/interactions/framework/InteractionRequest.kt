package com.abyssaldev.abyss.interactions.framework

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentChoiceSet
import com.abyssaldev.abyss.interactions.models.InteractionMember
import net.dv8tion.jda.api.entities.Guild
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.TextChannel

class InteractionRequest(guildId: String?, channelId: String?, rawMember: InteractionMember, val arguments: InteractionCommandArgumentChoiceSet) {
    var guild: Guild?
    var channel: TextChannel?
    var member: Member?
    var memberId: String

    init {
        val discord = AbyssEngine.instance.discordEngine

        memberId = rawMember.user.id
        guild = if (guildId != null) { discord.getGuildById(guildId) } else { null }
        channel = if (channelId != null) { discord.getTextChannelById(channelId) } else { null }
        member = guild?.getMemberById(rawMember.user.id)
    }
}