package com.abyssaldev.abyss.interactions.framework.subcommands

import com.abyssaldev.abyss.interactions.framework.InteractionExecutable
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import java.util.*

abstract class InteractionSubcommand: InteractionCommandOption, InteractionExecutable {
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