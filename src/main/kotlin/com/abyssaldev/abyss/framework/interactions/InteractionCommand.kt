package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
import com.abyssaldev.commands.framework.common.CommandExecutable
import com.abyssaldev.commands.framework.common.Responder
import net.dv8tion.jda.api.MessageBuilder
import java.util.*

abstract class InteractionCommand: InteractionBase, CommandExecutable<InteractionCommandRequest>, Responder {
    open val options: Array<InteractionCommandOption> = emptyArray()

    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): MessageBuilder?
        = respond("There was an error processing your subcommand.")

    override fun toJsonMap(): HashMap<String, Any> = hashMapOf(
        "name" to name,
        "description" to description,
        "options" to options.toJsonArray()
    )
}