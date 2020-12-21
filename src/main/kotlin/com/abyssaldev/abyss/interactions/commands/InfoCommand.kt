package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import com.abyssaldev.abyss.interactions.framework.subcommands.InteractionSubcommandSimple

class InfoCommand : InteractionCommand() {
    override val name = "info"
    override val description = "Shows information about a user, or a role."
    override val options: Array<InteractionCommandOption> = arrayOf(
        InteractionSubcommandSimple("user", description = "Finds information about a user.", options = arrayOf(
            InteractionCommandArgument("user", description = "The user to view information about.", type = InteractionCommandArgumentType.User, isRequired = true)
        )) {
            return@InteractionSubcommandSimple respond("Info about user " + this.arguments[0].value)
        },
        InteractionSubcommandSimple("role", description = "Finds information about a role.", options = arrayOf(
            InteractionCommandArgument("role", description = "The role to view information about.", type = InteractionCommandArgumentType.Role, isRequired = true)
        )) {
            return@InteractionSubcommandSimple respond("Info about role " + this.arguments[0].value)
        }
    )
}