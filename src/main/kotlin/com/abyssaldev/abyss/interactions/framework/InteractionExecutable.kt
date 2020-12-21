package com.abyssaldev.abyss.interactions.framework

import com.abyssaldev.abyss.interactions.InteractionRequest
import net.dv8tion.jda.api.MessageBuilder

interface InteractionExecutable {
    fun invoke(call: InteractionRequest): MessageBuilder

    fun respond(responder: MessageBuilder.() -> Unit): MessageBuilder {
        val builder = MessageBuilder()
        responder(builder)
        return builder
    }

    fun respond(content: String): MessageBuilder {
        return MessageBuilder().setContent(content)
    }
}