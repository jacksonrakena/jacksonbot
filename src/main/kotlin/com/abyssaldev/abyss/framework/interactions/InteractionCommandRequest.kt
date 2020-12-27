package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.framework.common.ArgumentSet
import com.abyssaldev.abyss.framework.common.CommandRequest
import com.abyssaldev.abyss.framework.interactions.models.Interaction
import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.entities.Guild
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.TextChannel
import net.dv8tion.jda.api.entities.User

class InteractionCommandRequest(
    override val guild: Guild,
    override val channel: TextChannel,
    override val member: Member,
    override val user: User,
    override val rawArgs: List<String>,
    override val jda: JDA,
    val rawArgsNamed: HashMap<String, String>
) : CommandRequest() {
    override val args: ArgumentSet.Named by lazy {
        ArgumentSet.Named(rawArgsNamed, this)
    }

    companion object {
        internal fun create(rawArgs: HashMap<String, String>, model: Interaction, jda: JDA): InteractionCommandRequest {
            val guild = jda.getGuildById(model.guildId!!)!!
            return InteractionCommandRequest(
                channel = jda.getTextChannelById(model.channelId!!)!!,
                guild = guild,
                jda = jda,
                member = guild.getMemberById(model.member!!.user.id)!!,
                rawArgs = rawArgs.values.toList(),
                user = jda.getUserById(model.member!!.user.id)!!,
                rawArgsNamed = rawArgs
            )
        }
    }
}