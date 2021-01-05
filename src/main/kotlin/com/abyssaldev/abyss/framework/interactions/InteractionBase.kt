package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.util.JsonHashable
import com.abyssaldev.rowi.core.CommandBase
import com.fasterxml.jackson.annotation.JsonIgnore
import net.dv8tion.jda.api.EmbedBuilder
import net.dv8tion.jda.api.MessageBuilder

interface InteractionBase: CommandBase, JsonHashable {
    @get:JsonIgnore
    val guildLock: Long
        get() = -1

    @get:JsonIgnore
    val isGuildLocked
        get() = guildLock != (-1).toLong()

        fun respond(responder: MessageBuilder.() -> Unit): InteractionCommandResponse {
            val builder = MessageBuilder()
            responder(builder)
            return InteractionCommandResponse(true, "", builder)
        }

        fun respondEmbed(responder: EmbedBuilder.() -> Unit): InteractionCommandResponse
            = InteractionCommandResponse(true, "", MessageBuilder().embed(responder))

        fun respond(content: String): InteractionCommandResponse {
            return InteractionCommandResponse(true, "", MessageBuilder().setContent(content))
        }

        fun MessageBuilder.content(content: String) = apply {
            setContent(content)
        }

        fun MessageBuilder.embed(builder: EmbedBuilder.() -> Unit) = apply {
            setEmbed(EmbedBuilder().apply(builder).build())
        }

        fun EmbedBuilder.appendDescriptionLine(line: String) = apply {
            this.descriptionBuilder.appendLine(line)
        }
    }