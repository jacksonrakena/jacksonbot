package com.abyssaldev.abyss.framework.common

import com.abyssaldev.abyss.AppConfig
import net.dv8tion.jda.api.EmbedBuilder
import net.dv8tion.jda.api.MessageBuilder

interface CommandExecutable<T: CommandRequest> : CommandBase {
    fun canInvoke(call: T): String? = ""

    suspend fun invoke(call: T): MessageBuilder?

    fun respond(responder: MessageBuilder.() -> Unit): MessageBuilder {
        val builder = MessageBuilder()
        responder(builder)
        return builder
    }

    fun respondEmbed(responder: EmbedBuilder.() -> Unit): MessageBuilder = MessageBuilder().embed(responder)

    fun respond(content: String): MessageBuilder {
        return MessageBuilder().setContent(content)
    }

    fun MessageBuilder.content(content: String) = apply {
        setContent(content)
    }

    fun MessageBuilder.embed(builder: EmbedBuilder.() -> Unit) = apply {
        setEmbed(EmbedBuilder().apply(builder).apply {
            if (this.build().color == null && AppConfig.instance.appearance.defaultEmbedColorObject != null) {
                setColor(AppConfig.instance.appearance.defaultEmbedColorObject)
            }
        }.build())
    }

    fun EmbedBuilder.appendDescriptionLine(line: String) = apply {
        this.descriptionBuilder.appendLine(line)
    }
}