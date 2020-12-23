package com.abyssaldev.abyss.interactions.framework

import net.dv8tion.jda.api.MessageBuilder

interface InteractionExecutable {
    /**
     * Runs checks on an interaction request to determine whether this command can be executed in the current
     * context. Should return `null` if the command can be executed, or a [String] containing the reason why it cannot.
     */
    fun canInvoke(call: InteractionRequest): String? = ""

    fun invoke(call: InteractionRequest): MessageBuilder?

    fun respond(responder: MessageBuilder.() -> Unit): MessageBuilder {
        val builder = MessageBuilder()
        responder(builder)
        return builder
    }

    fun respond(content: String): MessageBuilder {
        return MessageBuilder().setContent(content)
    }
}