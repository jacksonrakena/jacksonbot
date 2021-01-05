package com.abyssaldev.abyss.framework.interactions.subcommands

import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.InteractionCommandResponse
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.util.JsonHashable
import com.abyssaldev.rowi.core.CommandResponse

class InteractionSubcommandSimple(override val name: String,
                                  override val description: String,
                                  override val options: Array<InteractionCommandArgument>,
                                  val onInvoke: InteractionCommandRequest.() -> InteractionCommandResponse
) : InteractionSubcommand(), JsonHashable {
    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): CommandResponse = onInvoke(call)
}