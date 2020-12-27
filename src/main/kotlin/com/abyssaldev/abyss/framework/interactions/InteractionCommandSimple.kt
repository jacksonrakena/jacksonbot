package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
import net.dv8tion.jda.api.MessageBuilder

class InteractionCommandSimple(override val name: String,
                               override val description: String,
                               override val options: Array<InteractionCommandOption>,
                               val onInvoke: InteractionCommandRequest.() -> MessageBuilder
) : InteractionCommand() {
    override suspend fun invoke(call: InteractionCommandRequest): MessageBuilder = onInvoke(call)
}