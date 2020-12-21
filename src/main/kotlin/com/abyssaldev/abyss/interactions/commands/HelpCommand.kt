package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import net.dv8tion.jda.api.MessageBuilder

class HelpCommand : InteractionCommand() {
    override val name = "help"
    override val description: String = "Shows a list of available commands."

    override fun invoke(call: InteractionRequest): MessageBuilder {
        val commands = AbyssEngine.instance.interactions.getAllCommands()

        return respond {
            embed {
                setTitle("Commands")
                appendDescription("Remember, you can type `/` and commands will show up above your chat bar.")
                addField("Slash commands", commands.joinToString(", ") {
                    "`/" + it.name + "`"
                }, true)
            }
        }
    }
}