package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentChoiceSet
import com.abyssaldev.abyss.interactions.models.InteractionMember
import net.dv8tion.jda.api.entities.Guild
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.TextChannel

class InteractionRequest(guildId: String?, channelId: String?, rawMember: InteractionMember?, val arguments: InteractionCommandArgumentChoiceSet) {
    var guild: Guild?
    var channel: TextChannel?
    var member: Member?

    init {
        val discord = AbyssEngine.instance.discordEngine

        guild = if (guildId != null) { discord.getGuildById(guildId) } else { null }
        channel = if (channelId != null) { discord.getTextChannelById(channelId) } else { null }
        member = if (rawMember?.user?.id != null) { guild?.getMemberById(rawMember.user.id) } else { null }
    }
}