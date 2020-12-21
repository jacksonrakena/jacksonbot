package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.commands.models.arguments.InteractionCommandOption
import net.dv8tion.jda.api.MessageBuilder

class InteractionCommandSimple(override val name: String,
                               override val description: String,
                               override val options: Array<InteractionCommandOption>,
                               val onInvoke: InteractionRequest.() -> MessageBuilder
) : InteractionCommand() {
    override fun invoke(call: InteractionRequest): MessageBuilder = onInvoke(call)
}