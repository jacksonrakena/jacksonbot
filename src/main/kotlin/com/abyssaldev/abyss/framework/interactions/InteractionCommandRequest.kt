package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.framework.interactions.models.Interaction
import com.abyssaldev.rowi.core.ArgumentSet
import com.abyssaldev.rowi.core.CommandRequest
import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.entities.Guild
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.TextChannel
import net.dv8tion.jda.api.entities.User

class InteractionCommandRequest(
    val guild: Guild,
    val channel: TextChannel,
    val member: Member,
    val user: User,
    override val rawArgs: List<String>,
    val jda: JDA,
    val rawArgsNamed: HashMap<String, String>,
    override val flags: MutableList<String> = mutableListOf(),
    override var rawString: String = ""
) : CommandRequest() {
    val args: ArgumentSet.Named by lazy {
        ArgumentSet.Named(rawArgsNamed, this)
    }

    val botMember: Member by lazy {
        guild.getMemberById(user.id)!!
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