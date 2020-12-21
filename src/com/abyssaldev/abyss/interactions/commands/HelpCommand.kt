package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.commands.models.InteractionCommand

class HelpCommand : InteractionCommand() {
    override val name = "help"
    override val description: String = "Shows a list of available commands."

    override fun invoke(call: InteractionRequest): InteractionResponse {
        val commands = AbyssApplication.instance.interactions.getAllCommands()

        return respond(StringBuilder().apply {
            appendLine("Remember, you can type `/` and commands will show up above your chat bar.")
            appendLine()
            appendLine(commands.joinToString(", ") {
                "`/" + it.name + "`"
            })
        }.toString())
    }
}