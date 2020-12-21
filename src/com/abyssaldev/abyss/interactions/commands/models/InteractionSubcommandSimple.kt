package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.abs.JsonHashable

class InteractionSubcommandSimple(override val name: String,
                                  override val description: String,
                                  override val options: Array<InteractionCommandArgument>,
                                  val onInvoke: InteractionRequest.() -> InteractionResponse
) : InteractionSubcommand(), JsonHashable {
    override fun invoke(call: InteractionRequest): InteractionResponse = onInvoke(call)
}