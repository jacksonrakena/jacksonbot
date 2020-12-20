package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.abs.InteractionCommand

class FunctionCommand(override val name: String, override val description: String, private val response: InteractionRequest.() -> InteractionResponse) : InteractionCommand {
    override fun invoke(call: InteractionRequest): InteractionResponse = response(call)
}