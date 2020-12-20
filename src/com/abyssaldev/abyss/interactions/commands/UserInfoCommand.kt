package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.InteractionCommandArgumentType
import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.abs.InteractionCommand

class UserInfoCommand : InteractionCommand {
    override val name = "user"
    override val description = "Shows information about a user."
    override val arguments: Array<InteractionCommandArgument>? = arrayOf(
        InteractionCommandArgument("user", description = "The user to view information about.", type = InteractionCommandArgumentType.User, isRequired = true)
    )
    override val guildLock = 385902350432206849
    override fun invoke(call: InteractionRequest): InteractionResponse {
        val userId = call.arguments[0].value
        return respond("Information about $userId")
    }
}