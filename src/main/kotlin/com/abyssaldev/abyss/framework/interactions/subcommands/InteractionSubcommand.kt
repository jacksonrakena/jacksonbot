package com.abyssaldev.abyss.framework.interactions.subcommands

import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
import com.abyssaldev.rowi.core.CommandExecutable
import java.util.*

abstract class InteractionSubcommand: InteractionCommandOption, CommandExecutable<InteractionCommandRequest> {
    override val type: InteractionCommandArgumentType = InteractionCommandArgumentType.Subcommand
    abstract val options: Array<InteractionCommandArgument>

    override fun toJsonMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description,
            "type" to type.raw
        )
        if (options.any()) {
            hashMapInit["options"] = options.toJsonArray()
        }
        return hashMapInit
    }
}