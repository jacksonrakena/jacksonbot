package com.abyssaldev.abyss.framework.interactions.subcommands

import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
import java.util.*

class InteractionSubcommandGroup(
    override val name: String,
    override val description: String,
    val subcommands: Array<InteractionSubcommand> = emptyArray()
) : InteractionCommandOption {
    override val type: InteractionCommandArgumentType = InteractionCommandArgumentType.SubcommandGroup

    override fun toJsonMap(): HashMap<String, Any> = hashMapOf(
        "name" to name,
        "description" to description,
        "type" to type.raw,
        "options" to subcommands.toJsonArray()
    )
}