package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse

class InteractionCommandSimple(override val name: String,
                               override val description: String,
                               override val options: Array<InteractionCommandOption>,
                               val onInvoke: InteractionRequest.() -> InteractionResponse
) : InteractionCommand() {
    override fun invoke(call: InteractionRequest): InteractionResponse = onInvoke(call)
}