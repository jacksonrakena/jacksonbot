package com.abyssaldev.abyss.interactions.commands.models.subcommands

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.abs.JsonHashable
import com.abyssaldev.abyss.interactions.commands.models.arguments.InteractionCommandArgument
import net.dv8tion.jda.api.MessageBuilder

class InteractionSubcommandSimple(override val name: String,
                                  override val description: String,
                                  override val options: Array<InteractionCommandArgument>,
                                  val onInvoke: InteractionRequest.() -> MessageBuilder
) : InteractionSubcommand(), JsonHashable {
    override fun invoke(call: InteractionRequest): MessageBuilder = onInvoke(call)
}