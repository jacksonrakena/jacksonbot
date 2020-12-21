package com.abyssaldev.abyss.interactions.commands.models

import java.util.HashMap

abstract class InteractionSubcommand: InteractionCommandOption, InteractionExecutable {
    override val type: InteractionCommandArgumentType = InteractionCommandArgumentType.Subcommand
    abstract val options: Array<InteractionCommandArgument>

    override fun createMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description,
            "type" to type.raw
        )
        if (options.any()) {
            hashMapInit["options"] = options.map {
                it.createMap()
            }
        }
        return hashMapInit
    }
}