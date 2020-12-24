package com.abyssaldev.abyss.interactions.framework.subcommands

import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import java.util.*

class InteractionSubcommandGroup(
    override val name: String,
    override val description: String,
    val subcommands: Array<InteractionSubcommand> = emptyArray()
) : InteractionCommandOption {
    override val type: InteractionCommandArgumentType = InteractionCommandArgumentType.SubcommandGroup

    override fun toJsonMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description,
            "type" to type.raw
        )
        if (subcommands.any()) {
            hashMapInit["options"] = subcommands.toJsonArray()
        }
        return hashMapInit
    }
}