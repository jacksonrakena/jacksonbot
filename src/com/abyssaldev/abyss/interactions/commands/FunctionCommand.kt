package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.Interaction
import com.abyssaldev.abyss.interactions.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.abs.InteractionCommand

class FunctionCommand(override val name: String, override val description: String, private val response: Interaction.() -> InteractionResponse) : InteractionCommand {
    override fun invoke(invocation: Interaction): InteractionResponse = response(invocation)
}