package com.abyssaldev.abyss.interactions.abs

import com.abyssaldev.abyss.interactions.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse

interface InteractionCommand {
    val name: String
    val description: String
    val guildLock: Long
        get() = -1
    val isGuildLocked
        get() = guildLock != (-1).toLong()
    val arguments: Array<InteractionCommandArgument>?
        get() = null

    fun invoke(call: InteractionRequest): InteractionResponse

    fun respond(content: String) = InteractionResponse.message(content)
}