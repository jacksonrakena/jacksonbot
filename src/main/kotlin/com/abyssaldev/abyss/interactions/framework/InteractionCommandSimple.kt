package com.abyssaldev.abyss.interactions.framework

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import net.dv8tion.jda.api.MessageBuilder

class InteractionCommandSimple(override val name: String,
                               override val description: String,
                               override val options: Array<InteractionCommandOption>,
                               val onInvoke: InteractionRequest.() -> MessageBuilder
) : InteractionCommand() {
    override fun invoke(call: InteractionRequest): MessageBuilder = onInvoke(call)
}