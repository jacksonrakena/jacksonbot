package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.InteractionCommandResponse

class TextCommand(override val name: String, override val description: String, private val response: String) :
    InteractionCommand() {
    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): InteractionCommandResponse = respond(response)
}