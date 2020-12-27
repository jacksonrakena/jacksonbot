package com.abyssaldev.abyss.framework.interactions.subcommands

import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.util.JsonHashable
import net.dv8tion.jda.api.MessageBuilder

class InteractionSubcommandSimple(override val name: String,
                                  override val description: String,
                                  override val options: Array<InteractionCommandArgument>,
                                  val onInvoke: InteractionCommandRequest.() -> MessageBuilder
) : InteractionSubcommand(), JsonHashable {
    override suspend fun invoke(call: InteractionCommandRequest): MessageBuilder = onInvoke(call)
}