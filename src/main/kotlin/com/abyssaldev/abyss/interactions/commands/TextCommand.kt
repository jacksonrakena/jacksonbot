package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import net.dv8tion.jda.api.MessageBuilder

class TextCommand(override val name: String, override val description: String, private val response: String) :
    InteractionCommand() {
    override suspend fun invoke(call: InteractionRequest): MessageBuilder = respond(response)
}