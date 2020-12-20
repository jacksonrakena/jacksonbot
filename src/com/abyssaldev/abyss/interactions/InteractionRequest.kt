package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.AbyssApplication
import net.dv8tion.jda.api.entities.Guild
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.TextChannel

class InteractionRequest(private val guildId: String?, private val channelId: String?, private val rawMember: InteractionMember?, val arguments: Array<InteractionCommandOption>) {
    var guild: Guild?
    var channel: TextChannel?
    var member: Member?

    init {
        val discord = AbyssApplication.instance.discordEngine

        guild = if (guildId != null) { discord.getGuildById(guildId) } else { null }
        channel = if (channelId != null) { discord.getTextChannelById(channelId) } else { null }
        member = if (rawMember?.user?.id != null) { guild?.getMemberById(rawMember.user.id) } else { null }
    }
}