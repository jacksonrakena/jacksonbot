package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.commands.models.InteractionCommand

class TextCommand(override val name: String, override val description: String, private val response: String) :
    InteractionCommand() {
    override fun invoke(call: InteractionRequest): InteractionResponse = InteractionResponse(content = response)
}