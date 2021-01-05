package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
import com.abyssaldev.rowi.core.CommandExecutable
import com.abyssaldev.rowi.core.CommandResponse
import net.dv8tion.jda.api.MessageBuilder
import java.util.*

abstract class InteractionCommand: InteractionBase, CommandExecutable<InteractionCommandRequest> {
    open val options: Array<InteractionCommandOption> = emptyArray()

    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): CommandResponse
        = InteractionCommandResponse(true, "", MessageBuilder().setContent("There was an internal error."))

    override fun toJsonMap(): HashMap<String, Any> = hashMapOf(
        "name" to name,
        "description" to description,
        "options" to options.toJsonArray()
    )
}