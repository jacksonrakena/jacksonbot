package com.abyssaldev.abyss.interactions.framework.subcommands

import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.util.JsonHashable
import net.dv8tion.jda.api.MessageBuilder

class InteractionSubcommandSimple(override val name: String,
                                  override val description: String,
                                  override val options: Array<InteractionCommandArgument>,
                                  val onInvoke: InteractionRequest.() -> MessageBuilder
) : InteractionSubcommand(), JsonHashable {
    override fun invoke(call: InteractionRequest): MessageBuilder = onInvoke(call)
}