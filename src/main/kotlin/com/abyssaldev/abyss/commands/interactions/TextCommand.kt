package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import net.dv8tion.jda.api.MessageBuilder

class TextCommand(override val name: String, override val description: String, private val response: String) :
    InteractionCommand() {
    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): MessageBuilder = respond(response)
}