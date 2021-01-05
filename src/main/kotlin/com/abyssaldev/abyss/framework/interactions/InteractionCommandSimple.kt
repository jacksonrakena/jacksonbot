package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption

class InteractionCommandSimple(override val name: String,
                               override val description: String,
                               override val options: Array<InteractionCommandOption>,
                               val onInvoke: InteractionCommandRequest.() -> InteractionCommandResponse
) : InteractionCommand() {
    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): InteractionCommandResponse = onInvoke(call)
}