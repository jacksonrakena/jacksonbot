package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import net.dv8tion.jda.api.MessageBuilder

class HelpCommand : InteractionCommand() {
    override val name = "help"
    override val description: String = "Shows a list of available commands."

    override fun invoke(call: InteractionRequest): MessageBuilder {
        val commands = AbyssEngine.instance.interactions.getAllCommands()

        return respond {
            setContent(StringBuilder().apply {
                appendLine("Remember, you can type `/` and commands will show up above your chat bar.")
                appendLine()
                appendLine(commands.joinToString(", ") {
                    "`/" + it.name + "`"
                })
            }.toString())
        }
    }
}