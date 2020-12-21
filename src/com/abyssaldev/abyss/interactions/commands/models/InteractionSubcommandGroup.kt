package com.abyssaldev.abyss.interactions.commands.models

import java.util.HashMap

class InteractionSubcommandGroup(
    override val name: String,
    override val description: String,
    val subcommands: Array<InteractionSubcommand> = emptyArray()
) : InteractionCommandOption {
    override val type: InteractionCommandArgumentType = InteractionCommandArgumentType.SubcommandGroup

    override fun createMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description,
            "type" to type.raw
        )
        if (subcommands.any()) {
            hashMapInit["options"] = subcommands.map {
                it.createMap()
            }
        }
        return hashMapInit
    }
}